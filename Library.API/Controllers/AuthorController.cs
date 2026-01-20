using Library.Services.Interfaces;
using Library.Shared.DTOs.ApiResponses;
using Library.Shared.DTOs.Author;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorService _service;

        public AuthorsController(IAuthorService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpGet("exception-test")]
        public IActionResult ThrowException()
        {
            throw new InvalidOperationException("This is a test exception");
        }


        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(typeof(AuthorListDto), 201)]
        [Authorize(Policy = "author.manage")]
        public async Task<IActionResult> CreateAuthor([FromBody] CreateAuthorDto dto, [FromQuery] int userId)
        {
            try
            {
                if (dto == null)
                    return BadRequest(ApiResponseHelper.Failure<CreateAuthorDto>("Author data is required."));

                var author = await _service.CreateAuthorAsync(dto, userId);
                return CreatedAtAction(
                    nameof(GetAuthorByIdQuery),
                    new { id = author.Id },
                    ApiResponseHelper.Success(author)
                );
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponseHelper.Failure<CreateAuthorDto>(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<CreateAuthorDto>(ex.Message));
            }
        }

        [HttpGet("query")]
        [Authorize(Policy = "author.manage")]
        public IActionResult ListAuthorsQuery()
        {
            try
            {
                var result = _service.ListAuthorsQuery().ToList();
                return Ok(ApiResponseHelper.Success(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<List<AuthorListDto>>(ex.Message));
            }
        }

        [HttpGet("query/{id}")]
        [Authorize(Policy = "author.manage")]
        public async Task<IActionResult> GetAuthorByIdQuery(int id)
        {
            try
            {
                var query = _service.GetAuthorByIdQuery(id);
                var author = await query.FirstOrDefaultAsync();
                if (author == null)
                    return NotFound(ApiResponseHelper.Failure<AuthorListDto>("Author not found"));

                return Ok(ApiResponseHelper.Success(author));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<AuthorListDto>(ex.Message));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "author.manage")]
        public async Task<IActionResult> EditAuthor(int id, [FromBody] UpdateAuthorDto dto, [FromQuery] int userId)
        {
            try
            {
                if (id != dto.Id)
                    return BadRequest(ApiResponseHelper.Failure<UpdateAuthorDto>("ID mismatch."));

                var success = await _service.EditAuthorAsync(dto, userId);
                if (!success)
                    return NotFound(ApiResponseHelper.Failure<UpdateAuthorDto>("Author not found"));

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<UpdateAuthorDto>(ex.Message));
            }
        }


        [HttpPut("archive/{id}")]
        [Authorize(Policy = "author.manage")]
        public async Task<IActionResult> ArchiveAuthor(int id, [FromQuery] int userId)
        {
            try
            {
                var success = await _service.ArchiveAuthorAsync(id, userId);
                if (!success)
                    return NotFound(ApiResponseHelper.Failure<AuthorListDto>("Author not found"));

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<AuthorListDto>(ex.Message));
            }
        }
    }
}