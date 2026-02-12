using Library.Common.DTOs.ApiResponseDtos;
using Library.Common.DTOs.LibraryDtos.Author;
using Library.Common.Helpers;
using Library.Common.StringConstants;
using Library.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = PermissionNames.AuthorManage)]
    public class AuthorController : ControllerBase
    {
        private readonly IAuthorService _service;

        public AuthorController(IAuthorService service)
        {
            _service = service;
        }

        [HttpPost]
        [ProducesResponseType(typeof(AuthorListDto), 201)]
        public async Task<IActionResult> CreateAuthor([FromBody] CreateAuthorDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(ApiResponseHelper.Failure<CreateAuthorDto>("Author data is required."));

                int userId = UserClaimHelper.GetUserClaim(User);
                var author = await _service.CreateAuthorAsync(dto, userId);

                return CreatedAtAction(
                    nameof(GetAuthorByIdQuery),
                    new { id = author.Id },
                    ApiResponseHelper.Success(author)
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponseHelper.Failure<object>(ex.Message));
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
        public async Task<IActionResult> EditAuthor(int id, [FromBody] UpdateAuthorDto dto)
        {
            try
            {
                if (id != dto.Id)
                    return BadRequest(ApiResponseHelper.Failure<UpdateAuthorDto>("ID mismatch."));

                int userId = UserClaimHelper.GetUserClaim(User);
                var success = await _service.EditAuthorAsync(dto, userId);

                if (!success)
                    return NotFound(ApiResponseHelper.Failure<UpdateAuthorDto>("Author not found"));

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponseHelper.Failure<object>(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<UpdateAuthorDto>(ex.Message));
            }
        }

        [HttpPut("archive/{id}")]
        public async Task<IActionResult> ArchiveAuthor(int id)
        {
            try
            {
                int userId = UserClaimHelper.GetUserClaim(User);
                var success = await _service.ArchiveAuthorAsync(id, userId);

                if (!success)
                    return NotFound(ApiResponseHelper.Failure<AuthorListDto>("Author not found"));

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponseHelper.Failure<object>(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<AuthorListDto>(ex.Message));
            }
        }
    }
}