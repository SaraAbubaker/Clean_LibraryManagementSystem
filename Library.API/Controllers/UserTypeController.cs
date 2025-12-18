using Library.Services.Interfaces;
using Library.Shared.DTOs.ApiResponses;
using Library.Shared.DTOs.UserType;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserTypeController : ControllerBase
    {
        private readonly IUserTypeService _service;

        public UserTypeController(IUserTypeService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserType([FromBody] CreateUserTypeDto dto, [FromQuery] int createdByUserId)
        {
            try
            {
                var created = await _service.CreateUserTypeAsync(dto, createdByUserId);
                return Ok(ApiResponseHelper.Success(created));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<CreateUserTypeDto>(ex.Message));
            }
        }

        [HttpGet("query")]
        public IActionResult GetAllUserTypesQuery()
        {
            try
            {
                var query = _service.GetAllUserTypesQuery().ToList();
                return Ok(ApiResponseHelper.Success(query));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<List<UserTypeListDto>>(ex.Message));
            }
        }

        [HttpGet("query/{id}")]
        public IActionResult GetUserTypeByIdQuery(int id)
        {
            try
            {
                var query = _service.GetUserTypeByIdQuery(id);
                var userType = query.FirstOrDefault();
                if (userType == null)
                    return NotFound(ApiResponseHelper.Failure<UserTypeListDto>("User type not found"));

                return Ok(ApiResponseHelper.Success(userType));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<UserTypeListDto>(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserType([FromBody] UpdateUserTypeDto dto, int id, [FromQuery] int userId)
        {
            try
            {
                var updated = await _service.UpdateUserTypeAsync(dto, userId, id);
                return Ok(ApiResponseHelper.Success(updated));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<UpdateUserTypeDto>(ex.Message));
            }
        }

        [HttpPut("archive/{id}")]
        public async Task<IActionResult> ArchiveUserType(int id, [FromQuery] int userId)
        {
            try
            {
                await _service.ArchiveUserTypeAsync(id, userId);
                return Ok(ApiResponseHelper.Success(new { Message = "User type archived successfully." }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }
    }
}
