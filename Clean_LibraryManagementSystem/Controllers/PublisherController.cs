using Library.Services.Interfaces;
using Library.Shared.DTOs.ApiResponses;
using Library.Shared.DTOs.Publisher;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublisherController : ControllerBase
    {
        private readonly IPublisherService _service;

        public PublisherController(IPublisherService service)
        {
            _service = service;
        }


        [HttpPost]
        public async Task<IActionResult> CreatePubliser([FromBody] CreatePublisherDto dto, [FromQuery] int createdByUserId)
        {
            try
            {
                var created = await _service.CreatePublisherAsync(dto, createdByUserId);
                return Ok(ApiResponseHelper.Success(created));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<CreatePublisherDto>(ex.Message));
            }
        }

        [HttpGet("query")]
        public IActionResult GetAllPublisersQuery()
        {
            try
            {
                var query = _service.GetAllPublishersQuery().ToList();
                return Ok(ApiResponseHelper.Success(query));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<List<PublisherListDto>>(ex.Message));
            }
        }

        [HttpGet("query/{id}")]
        public IActionResult GetPubliserByIdQuery(int id)
        {
            try
            {
                var query = _service.GetPublisherByIdQuery(id);
                var publisher = query.FirstOrDefault();
                if (publisher == null)
                    return NotFound(ApiResponseHelper.Failure<PublisherListDto>("Publisher not found"));

                return Ok(ApiResponseHelper.Success(publisher));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<PublisherListDto>(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePubliser([FromBody] UpdatePublisherDto dto, int id, [FromQuery] int userId)
        {
            try
            {
                var updated = await _service.UpdatePublisherAsync(dto, userId, id);
                return Ok(ApiResponseHelper.Success(updated));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<UpdatePublisherDto>(ex.Message));
            }
        }

        [HttpPut("archive/{id}")]
        public async Task<IActionResult> ArchivePubliser(int id, [FromQuery] int? userId = null)
        {
            try
            {
                if (!userId.HasValue)
                    return BadRequest(ApiResponseHelper.Failure<object>("UserId is required to archive a publisher."));

                var success = await _service.ArchivePublisherAsync(id, userId.Value);
                if (!success)
                    return NotFound(ApiResponseHelper.Failure<object>("Publisher not found"));

                return Ok(ApiResponseHelper.Success(new { Message = "Publisher archived successfully." }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }
    }
}
