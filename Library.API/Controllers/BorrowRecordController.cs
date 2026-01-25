using Library.Common.RabbitMqMessages.ApiResponses;
using Library.Services.Interfaces;
using Library.Shared.DTOs.ApiResponses;
using Library.Shared.DTOs.BorrowRecord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BorrowRecordController : ControllerBase
    {
        private readonly IBorrowService _service;

        public BorrowRecordController(IBorrowService service)
        {
            _service = service;
        }

        [HttpPost("borrow")]
        [Authorize(Policy = "borrow.basic")]
        public async Task<IActionResult> BorrowBook(RequestBorrowDto dto, [FromQuery] int userId)
        {
            try
            {
                var borrow = await _service.BorrowBookAsync(dto, userId);
                return Ok(ApiResponseHelper.Success(borrow));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<RequestBorrowDto>(ex.Message));
            }
        }

        [HttpPost("return/{borrowRecordId}")]
        [Authorize(Policy = "borrow.basic")]
        public async Task<IActionResult> ReturnBook(int borrowRecordId, [FromQuery] int userId)
        {
            try
            {
                var success = await _service.ReturnBookAsync(borrowRecordId, userId);
                if (!success)
                    return NotFound(ApiResponseHelper.Failure<object>("Borrow record not found"));

                return Ok(ApiResponseHelper.Success(new { Message = "Book returned successfully." }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [HttpGet("query")]
        [Authorize(Policy = "borrow.manage")]
        public IActionResult GetBorrowDetailsQuery()
        {
            try
            {
                var query = _service.GetBorrowDetailsQuery().ToList();
                return Ok(ApiResponseHelper.Success(query));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<List<BorrowListDto>>(ex.Message));
            }
        }

        [HttpGet("query/overdue")]
        [Authorize(Policy = "borrow.manage")]
        public IActionResult GetOverdueRecordsQuery()
        {
            try
            {
                var query = _service.GetOverdueRecordsQuery().ToList();
                return Ok(ApiResponseHelper.Success(query));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<List<BorrowListDto>>(ex.Message));
            }
        }
    }
}