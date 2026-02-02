using Library.Common.DTOs.ApiResponseDtos;
using Library.Common.DTOs.LibraryDtos.BorrowRecord;
using Library.Common.Helpers;
using Library.Common.StringConstants;
using Library.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = PermissionNames.BorrowBasic)]
    public class BorrowRecordController : ControllerBase
    {
        private readonly IBorrowService _service;

        public BorrowRecordController(IBorrowService service)
        {
            _service = service;
        }

        [HttpPost("borrow")]
        public async Task<IActionResult> BorrowBook(RequestBorrowDto dto)
        {
            try
            {
                int userId = UserClaimHelper.GetUserClaim(User);
                var borrow = await _service.BorrowBookAsync(dto, userId);
                return Ok(ApiResponseHelper.Success(borrow));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponseHelper.Failure<object>(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<RequestBorrowDto>(ex.Message));
            }
        }

        [HttpPost("return/{borrowRecordId}")]
        public async Task<IActionResult> ReturnBook(int borrowRecordId)
        {
            try
            {
                int userId = UserClaimHelper.GetUserClaim(User);
                var success = await _service.ReturnBookAsync(borrowRecordId, userId);

                if (!success)
                    return NotFound(ApiResponseHelper.Failure<object>("Borrow record not found"));

                return Ok(ApiResponseHelper.Success(new { Message = "Book returned successfully." }));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponseHelper.Failure<object>(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [HttpGet("query")]
        [Authorize(Policy = PermissionNames.BorrowManage)]
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
        [Authorize(Policy = PermissionNames.BorrowManage)]
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