using Library.Common.RabbitMqMessages.UserMessages;
using Library.Services.Services;
using Library.Shared.DTOs.ApiResponses;
using Library.UserAPI.Interfaces;
using Library.UserAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Library.UserAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;

        public UserController(IUserService service, IConfiguration config, IAuthService authService)
        {
            _service = service;
            _config = config;
            _authService = authService;
        }

        [AllowAnonymous]
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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser(LoginUserMessage dto)
        {
            try
            {
                var loginResult = await _service.LoginUserAsync(dto);
                if (loginResult == null)
                    return Unauthorized(ApiResponseHelper.Failure<LoginUserMessage>("Invalid credentials"));

                // Build claims from loginResult.User
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, loginResult.User.Id.ToString()),
                    new Claim(ClaimTypes.Name, loginResult.User.Username),
                    new Claim(ClaimTypes.Email, loginResult.User.Email),
                    new Claim(ClaimTypes.Role, loginResult.User.UserRole)
                };

                var jwtKey = _config["Jwt:Key"]
                    ?? throw new InvalidOperationException("Jwt:Key is not configured.");
                var expiresInMinutes = int.Parse(_config["Jwt:ExpiresInMinutes"] ?? "60");

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(ApiResponseHelper.Success(new
                {
                    token = tokenString,
                    loggedInAt = loginResult.LoggedInAt,
                    role = loginResult.User.UserRole,
                    borrowCount = loginResult.BorrowedBooksCount
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<LoginUserMessage>(ex.Message));
            }
        }

        [Authorize(AuthenticationSchemes = "LocalJWT")]
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

        [Authorize(AuthenticationSchemes = "LocalJWT")]
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

        [Authorize(AuthenticationSchemes = "LocalJWT")]
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

        // Only Admins can deactivate users
        [Authorize(Roles = "Admin", AuthenticationSchemes = "LocalJWT")]
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

        // Only Admins can reactivate users
        [Authorize(Roles = "Admin", AuthenticationSchemes = "LocalJWT")]
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

        // Only Admins can archive users
        [Authorize(Roles = "Admin", AuthenticationSchemes = "LocalJWT")]
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