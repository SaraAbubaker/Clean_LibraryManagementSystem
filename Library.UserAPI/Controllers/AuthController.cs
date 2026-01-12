using Library.Common.RabbitMqMessages.TokenMessages;
using Library.Common.RabbitMqMessages.UserMessages;
using Library.UserAPI.Data;
using Library.UserAPI.Interfaces;
using Library.UserAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.UserAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(ApplicationDbContext context, IAuthService authService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _authService = authService;
            _userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserMessage request)
        {
            // Find user by username or email
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == request.UsernameOrEmail || u.Email == request.UsernameOrEmail);

            if (user == null)
                return Unauthorized("Invalid credentials");

            // Verify password using UserManager (hash check)
            var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!validPassword)
                return Unauthorized("Invalid credentials");

            // Get roles
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            var userMessage = new UserListMessage
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                UserRole = role
            };

            var jwtToken = _authService.GenerateJwtToken(userMessage);
            var refreshToken = _authService.GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(_authService.GetRefreshTokenLifetimeDays()),
                IsRevoked = false
            });
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token = jwtToken,
                refreshToken = refreshToken
            });
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token");

            var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
            if (user == null)
                return Unauthorized("User not found");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            var userMessage = new UserListMessage
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                UserRole = role
            };

            var newJwt = _authService.GenerateJwtToken(userMessage);
            var newRefresh = _authService.GenerateRefreshToken();

            // revoke old token
            storedToken.IsRevoked = true;

            // save new token
            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = newRefresh,
                ExpiresAt = DateTime.UtcNow.AddDays(_authService.GetRefreshTokenLifetimeDays()),
                IsRevoked = false
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                token = newJwt,
                refreshToken = newRefresh
            });
        }


        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RevokeTokenRequest request)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (storedToken == null)
                return NotFound("Refresh token not found");

            storedToken.IsRevoked = true;
            await _context.SaveChangesAsync();

            return Ok("Refresh token revoked");
        }
    }
}
