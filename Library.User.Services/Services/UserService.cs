using Library.Common.Helpers;
using Library.Common.RabbitMqMessages.UserMessages;
using Library.Common.Exceptions;
using Library.User.Domain.Data;
using Library.User.Entities;
using Library.User.Services.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Library.User.Services.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;

        public UserService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IConfiguration config,
            IAuthService authService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _config = config;
            _authService = authService;
            _context = context;
        }

        public async Task<LoginUserResponseMessage> RegisterUserAsync(RegisterUserMessage dto)
        {
            Validate.ValidateModel(dto);

            var usernameNormalized = dto.Username.Trim();
            var emailInput = dto.Email.Trim();

            if (await _userManager.FindByNameAsync(usernameNormalized) != null)
                throw new InvalidOperationException("Username already taken.");

            if (await _userManager.FindByEmailAsync(emailInput) != null)
                throw new InvalidOperationException("Email already registered.");

            var user = new ApplicationUser
            {
                UserName = usernameNormalized,
                Email = emailInput,
                CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IsArchived = false,
                LockoutEnabled = false
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

            if (await _roleManager.RoleExistsAsync("Customer"))
                await _userManager.AddToRoleAsync(user, "Customer");

            //call LoginUserAsync with the new credentials
            var loginDto = new LoginUserMessage
            {
                UsernameOrEmail = dto.Username,
                Password = dto.Password
            };

            return await LoginUserAsync(loginDto);
        }

        public async Task<LoginUserResponseMessage> LoginUserAsync(LoginUserMessage dto)
        {
            // Validate input
            Validate.ValidateModel(dto);

            var input = dto.UsernameOrEmail.Trim();
            var password = dto.Password.Trim();

            // Resolve user by username OR email
            ApplicationUser? user;
            if (input.Contains("@"))
                user = await _userManager.FindByEmailAsync(input);
            else
                user = await _userManager.FindByNameAsync(input);

            if (user == null)
            {
                if (input.Contains("@"))
                    throw new BadRequestException("Invalid email.");
                else
                    throw new BadRequestException("Invalid username.");
            }

            if (user.IsArchived)
                throw new UnauthorizedAccessException("This account has been archived and cannot log in.");

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
                throw new UnauthorizedAccessException("This account is currently deactivated or locked out.");

            // Attempt sign-in using the resolved user object
            var result = await _signInManager.CheckPasswordSignInAsync(
                user, password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                if (string.IsNullOrWhiteSpace(password))
                    throw new BadRequestException("Password cannot be empty.");
                else
                    throw new BadRequestException("Invalid password.");
            }

            // Get roles
            var roles = await _userManager.GetRolesAsync(user) ?? new List<string>();
            var role = roles.FirstOrDefault() ?? "Unknown";

            // Fetch permissions for this user’s role
            var permissions = await _context.RolePermissions
                .Where(rp => roles.Contains(rp.Role.Name ?? string.Empty))
                .Select(rp => rp.Permission.PermissionName)
                .Distinct()
                .ToListAsync();

            // Generate refresh token
            var refreshToken = _authService.GenerateRefreshToken();
            var hashedRefresh = _authService.HashToken(refreshToken);

            // Persist refresh token securely
            var refreshEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = hashedRefresh,
                ExpiresAt = DateTime.UtcNow.AddDays(_authService.GetRefreshTokenLifetimeDays()),
                IsRevoked = false
            };
            _context.RefreshTokens.Add(refreshEntity);
            await _context.SaveChangesAsync();

            // Build response DTO
            var response = new LoginUserResponseMessage
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                UserRole = role,
                LoggedInAt = DateTime.UtcNow,
                RefreshToken = refreshToken,
                Permissions = permissions
            };

            // Generate JWT
            response.AccessToken = _authService.GenerateJwtToken(response);

            return response;
        }

        public IQueryable<UserListMessage> GetAllUsersQuery()
        {
            return _userManager.Users
                .AsNoTracking()
                .Select(u => new UserListMessage
                {
                    Id = u.Id,
                    Username = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    IsArchived = u.IsArchived,
                    LockoutEnabled = u.LockoutEnabled,
                    LockoutEnd = u.LockoutEnd,
                    Status = u.IsArchived ? "Archived"
                        : (u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow
                            ? "Deactivated"
                            : "Active")
                });
        }

        public IQueryable<UserListMessage> GetUserByIdQuery(int id)
        {
            Validate.Positive(id, nameof(id));

            return _userManager.Users
                .Where(u => u.Id == id)
                .AsNoTracking()
                .Select(u => new UserListMessage
                {
                    Id = u.Id,
                    Username = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    IsArchived = u.IsArchived,
                    LockoutEnabled = u.LockoutEnabled,
                    LockoutEnd = u.LockoutEnd,
                    Status = u.IsArchived ? "Archived"
                        : (u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow
                            ? "Deactivated"
                            : "Active")
                });
        }

        public async Task<LogoutUserMessage> LogoutUserAsync(int userId, string token)
        {
            Validate.Positive(userId, nameof(userId));
            Validate.NotEmpty(token, nameof(token));

            var user = await _userManager.FindByIdAsync(userId.ToString())
                       ?? throw new InvalidOperationException("User not found.");

            // Hash the incoming token
            var Token = HashToken(token);

            // Look up the refresh token by hash
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == Token);

            if (storedToken == null)
                throw new InvalidOperationException("Refresh token not found.");

            // Mark it revoked
            storedToken.IsRevoked = true;
            await _context.SaveChangesAsync();

            // Clear sign-in state (cookies, Identity session)
            await _signInManager.SignOutAsync();

            // Build logout message
            var dto = user.Adapt<LogoutUserMessage>();
            dto.LoggedOutAt = DateTime.UtcNow;
            dto.Status = "Success";

            return dto;
        }

        public async Task<UserListMessage> DeactivateUserAsync(int id, int performedByUserId)
        {
            Validate.Positive(id, nameof(id));
            Validate.Positive(performedByUserId, nameof(performedByUserId));

            var user = await _userManager.FindByIdAsync(id.ToString())
                       ?? throw new InvalidOperationException("User not found.");

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);

            user.DeactivatedByUserId = performedByUserId;
            user.DeactivatedDate = DateTimeOffset.UtcNow;
            user.LastModifiedByUserId = performedByUserId;
            user.LastModifiedDate = DateOnly.FromDateTime(DateTime.UtcNow);

            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var dto = user.Adapt<UserListMessage>();
            dto.UserRole = roles.FirstOrDefault() ?? "Unknown";

            return dto;
        }

        public async Task<UserListMessage> ReactivateUserAsync(int id, int performedByUserId)
        {
            Validate.Positive(id, nameof(id));
            Validate.Positive(performedByUserId, nameof(performedByUserId));

            var user = await _userManager.FindByIdAsync(id.ToString())
                       ?? throw new InvalidOperationException("User not found.");

            if (user.IsArchived)
                throw new InvalidOperationException("Archived users cannot be reactivated.");

            user.LockoutEnd = null;
            user.LockoutEnabled = false;

            user.DeactivatedByUserId = null;
            user.DeactivatedDate = null;
            user.LastModifiedByUserId = performedByUserId;
            user.LastModifiedDate = DateOnly.FromDateTime(DateTime.UtcNow);

            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var dto = user.Adapt<UserListMessage>();
            dto.UserRole = roles.FirstOrDefault() ?? "Unknown";

            return dto;
        }

        public async Task<UserListMessage> ArchiveUserAsync(int id, int performedByUserId)
        {
            Validate.Positive(id, nameof(id));
            Validate.Positive(performedByUserId, nameof(performedByUserId));

            var user = await _userManager.FindByIdAsync(id.ToString())
                       ?? throw new InvalidOperationException("User not found.");

            user.IsArchived = true;
            user.ArchivedByUserId = performedByUserId;
            user.ArchivedDate = DateOnly.FromDateTime(DateTime.UtcNow);
            user.LastModifiedByUserId = performedByUserId;
            user.LastModifiedDate = DateOnly.FromDateTime(DateTime.UtcNow);

            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var dto = user.Adapt<UserListMessage>();
            dto.UserRole = roles.FirstOrDefault() ?? "Unknown";

            return dto;
        }

        //Helper
        private static string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}