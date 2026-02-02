using Library.Common.DTOs.ApiResponseDtos;
using Library.Common.DTOs.LibraryDtos.Publisher;
using Library.Common.StringConstants;
using Library.Services.Interfaces;
using Library.Common.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = PermissionNames.PublisherBasic)]
    public class PublisherController : ControllerBase
    {
        private readonly IPublisherService _service;

        public PublisherController(IPublisherService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Policy = PermissionNames.PublisherManage)]
        public async Task<IActionResult> CreatePublisher([FromBody] CreatePublisherDto dto)
        {
            try
            {
                int userId = UserClaimHelper.GetUserClaim(User);
                var created = await _service.CreatePublisherAsync(dto, userId);
                return Ok(ApiResponseHelper.Success(created));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponseHelper.Failure<object>(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<CreatePublisherDto>(ex.Message));
            }
        }

        [HttpGet("query")]
        public IActionResult GetAllPublishersQuery()
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
        public IActionResult GetPublisherByIdQuery(int id)
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
        [Authorize(Policy = PermissionNames.PublisherManage)]
        public async Task<IActionResult> UpdatePublisher([FromBody] UpdatePublisherDto dto, int id)
        {
            try
            {
                int userId = UserClaimHelper.GetUserClaim(User);
                var updated = await _service.UpdatePublisherAsync(dto, userId, id);
                return Ok(ApiResponseHelper.Success(updated));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponseHelper.Failure<object>(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<UpdatePublisherDto>(ex.Message));
            }
        }

        [HttpPut("archive/{id}")]
        [Authorize(Policy = PermissionNames.PublisherManage)]
        public async Task<IActionResult> ArchivePublisher(int id)
        {
            try
            {
                int userId = UserClaimHelper.GetUserClaim(User);
                var success = await _service.ArchivePublisherAsync(id, userId);

                if (!success)
                    return NotFound(ApiResponseHelper.Failure<object>("Publisher not found"));

                return Ok(ApiResponseHelper.Success(new { Message = "Publisher archived successfully." }));
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
    }
}