using Library.Common.RabbitMqMessages.UserMessages;
using Library.User.Domain.Data;
using Library.User.Services.Interfaces;
using Library.User.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Library.Common.DTOs.UserApiDtos.TokenDtos;

namespace Library.User.API.Controllers
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

        [Authorize(Policy = "auth.manage")]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var hashedInput = _authService.HashToken(request.RefreshToken);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == hashedInput);

            if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token");

            var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
            if (user == null)
                return Unauthorized("User not found");

            if (user.IsArchived)
                return Unauthorized("Archived accounts cannot refresh tokens.");

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
                return Unauthorized("Deactivated accounts cannot refresh tokens.");

            // Build response DTO directly (flattened)
            var newRefreshToken = _authService.GenerateRefreshToken();
            var response = new LoginUserResponseMessage
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                UserRole = "Admin", // role string no longer matters, perms drive access
                LoggedInAt = DateTime.UtcNow,
                RefreshToken = newRefreshToken
            };

            // Generate JWT using the flattened DTO
            response.AccessToken = _authService.GenerateJwtToken(response);

            // Hash and persist new refresh token
            var hashedNewRefresh = _authService.HashToken(newRefreshToken);

            // revoke old token
            storedToken.IsRevoked = true;

            // save new token
            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = hashedNewRefresh,
                ExpiresAt = DateTime.UtcNow.AddDays(_authService.GetRefreshTokenLifetimeDays()),
                IsRevoked = false
            });

            await _context.SaveChangesAsync();

            return Ok(response);
        }

        [Authorize(Policy = "auth.manage")]
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RevokeTokenRequest request)
        {
            var hashedInput = _authService.HashToken(request.RefreshToken);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == hashedInput);

            if (storedToken == null)
                return NotFound("Refresh token not found");

            storedToken.IsRevoked = true;
            await _context.SaveChangesAsync();

            return Ok("Refresh token revoked");
        }
    }
}
