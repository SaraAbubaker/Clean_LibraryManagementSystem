using Library.Services.Interfaces;
using Library.Shared.DTOs;
using Library.Shared.DTOs.ApiResponses;
using Library.Shared.DTOs.Book;
using Library.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Library.Shared.DTOs.SearchParamsDto;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _service;

        public BooksController(IBookService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookDto dto, [FromQuery] int userId)
        {
            try
            {
                var book = await _service.CreateBookAsync(dto, userId);
                return CreatedAtAction(
                    nameof(GetBookDetailsQuery),
                    new { id = book.Id },
                    ApiResponseHelper.Success(book)
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<CreateBookDto>(ex.Message));
            }
        }

        [HttpGet("query/{id}")]
        public async Task<IActionResult> GetBookDetailsQuery(int id)
        {
            try
            {
                var query = _service.GetBookDetailsQuery(id);
                var book = await query.FirstOrDefaultAsync(); // Execute query
                if (book == null)
                    return NotFound(ApiResponseHelper.Failure<BookListDto>("Book not found"));

                return Ok(ApiResponseHelper.Success(book));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<BookListDto>(ex.Message));
            }
        }

        [HttpGet("query/author/{authorId}")]
        public IActionResult GetBooksByAuthorQuery(int authorId)
        {
            try
            {
                var query = _service.GetBooksByAuthorQuery(authorId);
                return Ok(ApiResponseHelper.Success(query.ToList()));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<List<BookListDto>>(ex.Message));
            }
        }

        [HttpGet("query/category/{categoryId}")]
        public IActionResult GetBooksByCategoryQuery(int categoryId)
        {
            try
            {
                var query = _service.GetBooksByCategoryQuery(categoryId);
                return Ok(ApiResponseHelper.Success(query.ToList()));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<List<BookListDto>>(ex.Message));
            }
        }

        [HttpGet("query/search")]
        public async Task<IActionResult> SearchBooksQuery(
            [FromQuery] SearchBookParamsDto filters,
            [FromQuery] int page = 1,
            [FromQuery] BookSortBy sortBy = BookSortBy.Id,
            [FromQuery] SortDirection sortDir = SortDirection.Asc)
        {
            try
            {
                var searchDto = new SearchParamsDto
                {
                    Page = page,
                    SortBy = sortBy,
                    SortDir = sortDir
                };

                var pagedResult = await _service.SearchBooksQuery(filters, searchDto);

                return Ok(ApiResponseHelper.SuccessPaged(pagedResult));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, UpdateBookDto dto, [FromQuery] int userId)
        {
            try
            {
                if (id != dto.Id)
                    return BadRequest(ApiResponseHelper.Failure<UpdateBookDto>("ID mismatch."));

                var success = await _service.UpdateBookAsync(dto, userId);
                if (!success)
                    return NotFound(ApiResponseHelper.Failure<UpdateBookDto>("Book not found."));

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<UpdateBookDto>(ex.Message));
            }
        }

        [HttpPut("archive/{id}")]
        public async Task<IActionResult> ArchiveBook(int id, [FromQuery] int userId)
        {
            try
            {
                var success = await _service.ArchiveBookAsync(id, userId);
                if (!success)
                    return NotFound(ApiResponseHelper.Failure<BookListDto>("Book not found."));

                var query = _service.GetBookDetailsQuery(id);
                var book = await query.FirstOrDefaultAsync();

                return Ok(ApiResponseHelper.Success(book));
            }
            catch (NotFoundException)
            {
                return NotFound(ApiResponseHelper.Failure<BookListDto>("Book not found."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<BookListDto>(ex.Message));
            }
        }


    }
}
