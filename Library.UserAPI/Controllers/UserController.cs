using Library.Common.RabbitMqMessages.UserMessages;
using Library.Shared.DTOs.ApiResponses;
using Library.UserAPI.Interfaces;
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

        public UserController(IUserService service, IConfiguration config)
        {
            _service = service;
            _config = config;
        }

        // 🔓 Public: anyone can register
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

        // 🔓 Public: login issues JWT
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser(LoginUserMessage dto)
        {
            try
            {
                var user = await _service.LoginUserAsync(dto);
                if (user == null)
                    return Unauthorized(ApiResponseHelper.Failure<LoginUserMessage>("Invalid credentials"));

                // Use UserRole from your DTO
                var token = GenerateJwtToken(user.Username, user.Id.ToString(), user.UserRole);

                return Ok(ApiResponseHelper.Success(new { token }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<LoginUserMessage>(ex.Message));
            }
        }

        // 🔐 Protected: requires valid JWT
        [Authorize(AuthenticationSchemes = "LocalJWT,AzureAD")]
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

        // 🔐 Protected: requires valid JWT
        [Authorize(AuthenticationSchemes = "LocalJWT,AzureAD")]
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

        // 🔐 Protected: requires valid JWT
        // Only Admins can archive users
        [Authorize(Roles = "Admin", AuthenticationSchemes = "LocalJWT,AzureAD")]
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


        // 🔑 Helper: generate JWT
        private string GenerateJwtToken(string username, string userId, string role)
        {
            var jwtKey = _config["Jwt:Key"]
                         ?? throw new InvalidOperationException("Jwt:Key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim("userId", userId),
                new Claim(ClaimTypes.Role, role) // 🔑 role claim
            };

            var token = new JwtSecurityToken(
                issuer: "your-api",
                audience: "your-api-users",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}