using Library.Common.RabbitMqMessages.UserMessages;
using Library.Shared.DTOs.ApiResponses;
using Library.UserAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Library.UserAPI.Controllers
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

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(RegisterUserMessage dto)
        {
            try
            {
                var result = await _service.RegisterUserAsync(dto);
                return Ok(ApiResponseHelper.Success(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<RegisterUserMessage>(ex.Message));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser(LoginMessage dto)
        {
            try
            {
                var result = await _service.LoginUserAsync(dto);
                return Ok(ApiResponseHelper.Success(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<LoginMessage>(ex.Message));
            }
        }

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
                return BadRequest(ApiResponseHelper.Failure<List<UserListDto>>(ex.Message));
            }
        }

        [HttpGet("query/{id}")]
        public IActionResult GetUserByIdQuery(int id)
        {
            try
            {
                var query = _service.GetUserByIdQuery(id);
                var user = query.FirstOrDefault();
                if (user == null)
                    return NotFound(ApiResponseHelper.Failure<UserListDto>("User not found"));

                return Ok(ApiResponseHelper.Success(user));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<UserListDto>(ex.Message));
            }
        }

        [HttpPut("{id}")]
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
