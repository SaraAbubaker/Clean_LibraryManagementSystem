using Library.Common.RabbitMqMessages.UserTypeMessages;
using Library.Shared.DTOs.ApiResponses;
using Library.UserAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.UserAPI.Controllers
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

        // 🔐 Only Admins can create new user types
        [Authorize(Roles = "Admin", AuthenticationSchemes = "LocalJWT,AzureAD")]
        [HttpPost]
        public async Task<IActionResult> CreateUserType([FromBody] CreateUserTypeMessage dto, [FromQuery] int createdByUserId)
        {
            try
            {
                var created = await _service.CreateUserTypeAsync(dto, createdByUserId);
                return Ok(ApiResponseHelper.Success(created));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<CreateUserTypeMessage>(ex.Message));
            }
        }

        // 🔓 Any authenticated user can query user types
        [Authorize(AuthenticationSchemes = "LocalJWT,AzureAD")]
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
                return BadRequest(ApiResponseHelper.Failure<List<UserTypeListMessage>>(ex.Message));
            }
        }

        [Authorize(AuthenticationSchemes = "LocalJWT,AzureAD")]
        [HttpGet("query/{id}")]
        public IActionResult GetUserTypeByIdQuery(int id)
        {
            try
            {
                var query = _service.GetUserTypeByIdQuery(id);
                var userType = query.FirstOrDefault();
                if (userType == null)
                    return NotFound(ApiResponseHelper.Failure<UserTypeListMessage>("User type not found"));

                return Ok(ApiResponseHelper.Success(userType));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<UserTypeListMessage>(ex.Message));
            }
        }

        // 🔐 Only Admins can update user types
        [Authorize(Roles = "Admin", AuthenticationSchemes = "LocalJWT,AzureAD")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserType([FromBody] UpdateUserTypeMessage dto, int id, [FromQuery] int userId)
        {
            try
            {
                var updated = await _service.UpdateUserTypeAsync(dto, userId, id);
                return Ok(ApiResponseHelper.Success(updated));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<UpdateUserTypeMessage>(ex.Message));
            }
        }

        // 🔐 Only Admins can archive user types
        [Authorize(Roles = "Admin", AuthenticationSchemes = "LocalJWT,AzureAD")]
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