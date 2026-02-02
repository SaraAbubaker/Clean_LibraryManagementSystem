using Library.Common.DTOs.ApiResponseDtos;
using Library.Common.DTOs.UserApiDtos.UserDtos;
using Library.Common.Exceptions;
using Library.User.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.User.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserMessage dto)
        {
            try
            {
                var result = await _service.RegisterUserAsync(dto);
                return Ok(ApiResponseHelper.Success(result));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponseHelper.Failure<string>(ex.Message));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ApiResponseHelper.Failure<string>(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponseHelper.Failure<string>(ex.Message));
            }
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginUserMessage dto)
        {
            try
            {
                var loginResult = await _service.LoginUserAsync(dto);
                return Ok(ApiResponseHelper.Success(loginResult)); // Success wraps LoginUserResponseMessage
            }
            catch (BadRequestException ex)
            {
                return Unauthorized(ApiResponseHelper.Failure<LoginUserResponseMessage>(ex.Message)); // ❌ Fix type here
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponseHelper.Failure<LoginUserResponseMessage>(ex.Message)); // ❌ Fix type here
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<LoginUserResponseMessage>(ex.Message)); // ❌ Fix type here
            }
        }

        [Authorize(Policy = "user.basic")]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromQuery] int userId, [FromBody] string token)
        {
            try
            {
                var result = await _service.LogoutUserAsync(userId, token);
                return Ok(ApiResponseHelper.Success(result));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponseHelper.Failure<object>("User not found"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [Authorize(Policy = "user.manage")]
        [HttpGet("query")]
        public IActionResult GetAllUsersQuery()
        {
            try
            {
                var query = _service.GetAllUsersQuery().ToList();
                return Ok(ApiResponseHelper.Success(query));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<List<UserListMessage>>(ex.Message));
            }
        }

        [Authorize(Policy = "user.manage")]
        [HttpGet("query/{id}")]
        public IActionResult GetUserByIdQuery(int id)
        {
            try
            {
                var query = _service.GetUserByIdQuery(id);
                var user = query.FirstOrDefault();
                if (user == null)
                    return NotFound(ApiResponseHelper.Failure<UserListMessage>("User not found"));

                return Ok(ApiResponseHelper.Success(user));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<UserListMessage>(ex.Message));
            }
        }

        [Authorize(Policy = "user.manage")]
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int id, [FromQuery] int performedByUserId)
        {
            try
            {
                var result = await _service.DeactivateUserAsync(id, performedByUserId);
                return Ok(ApiResponseHelper.Success(result));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponseHelper.Failure<object>("User not found"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [Authorize(Policy = "user.manage")]
        [HttpPut("{id}/reactivate")]
        public async Task<IActionResult> ReactivateUser(int id, [FromQuery] int performedByUserId)
        {
            try
            {
                var result = await _service.ReactivateUserAsync(id, performedByUserId);
                return Ok(ApiResponseHelper.Success(result));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponseHelper.Failure<object>("User not found"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [Authorize(Policy = "user.manage")]
        [HttpPut("{id}/archive")]
        public async Task<IActionResult> ArchiveUser(int id, [FromQuery] int performedByUserId)
        {
            try
            {
                var result = await _service.ArchiveUserAsync(id, performedByUserId);
                return Ok(ApiResponseHelper.Success(result));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponseHelper.Failure<object>("User not found"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }
    }
}
