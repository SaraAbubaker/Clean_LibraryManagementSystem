using Library.Common.RabbitMqMessages.ApiResponses;
using Library.Services.Interfaces;
using Library.Shared.DTOs.ApiResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _service;

        public InventoryController(IInventoryService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Policy = "inventory.manage")]
        public async Task<IActionResult> CreateCopy(int bookId, [FromQuery] int userId)
        {
            try
            {
                var record = await _service.CreateCopyAsync(bookId, userId);
                return Ok(ApiResponseHelper.Success(record));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [HttpPost("return/{inventoryRecordId}")]
        [Authorize(Policy = "inventory.manage")]
        public async Task<IActionResult> ReturnCopy(int inventoryRecordId, [FromQuery] int userId)
        {
            try
            {
                var success = await _service.ReturnCopyAsync(inventoryRecordId, userId);

                if (!success)
                    return NotFound(ApiResponseHelper.Failure<object>("Inventory record not found."));

                return Ok(ApiResponseHelper.Success(new { Message = "Copy returned successfully." }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [HttpGet("book/{bookId}/query")]
        [Authorize(Policy = "inventory.manage")]
        public IActionResult ListCopiesQuery(int bookId)
        {
            try
            {
                var query = _service.ListCopiesForBookQuery(bookId).ToList();
                return Ok(ApiResponseHelper.Success(query));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [HttpGet("available/{bookId}/query")]
        [Authorize(Policy = "inventory.manage")]
        public IActionResult ListAvailableCopiesQuery(int bookId)
        {
            try
            {
                var query = _service.GetAvailableCopiesQuery(bookId).ToList();
                return Ok(ApiResponseHelper.Success(query));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [HttpPut("archive/{id}")]
        [Authorize(Policy = "inventory.manage")]
        public async Task<IActionResult> ArchiveCopy(int id, [FromQuery] int userId)
        {
            try
            {
                var success = await _service.ArchiveCopyAsync(id, userId);

                if (!success)
                    return BadRequest(ApiResponseHelper.Failure<object>("Copy cannot be removed (may be borrowed or not found)."));

                return Ok(ApiResponseHelper.Success(new { Message = "Copy removed successfully." }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }
    }
}