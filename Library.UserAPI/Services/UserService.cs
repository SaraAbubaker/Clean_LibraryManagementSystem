using Library.Common.RabbitMqMessages.UserMessages;
using Library.Services.Interfaces;
using Library.Shared.Exceptions;
using Library.Shared.Helpers;
using Library.UserAPI.Interfaces;
using Library.UserAPI.Models;
using Library.UserAPI.Repositories.UserRepo;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Library.UserAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IUserTypeRepository _userTypeRepo;
        private readonly IBorrowService _borrowService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _config;

        public UserService(
            IUserRepository userRepo,
            IUserTypeRepository userTypeRepo,
            IBorrowService borrowService,
            IPasswordHasher<User> passwordHasher,
            IConfiguration config)
        {
            _userRepo = userRepo;
            _userTypeRepo = userTypeRepo;
            _borrowService = borrowService;
            _passwordHasher = passwordHasher;
            _config = config;
        }

        public async Task<UserListMessage> RegisterUserAsync(RegisterUserMessage dto)
        {
            Validate.ValidateModel(dto);

            var usernameNormalized = dto.Username.Trim();
            var emailInput = dto.Email.Trim();

            var users = _userRepo.GetAll();

            if (users.Any(u => u.Username.ToLower() == usernameNormalized.ToLower()))
                throw new InvalidOperationException("Username already taken.");

            if (users.Any(u => u.Email.ToLower() == emailInput.ToLower()))
                throw new InvalidOperationException("Email already registered.");

            // Normal user type lookup (from UserTypeRepo, not UserRepo)
            var userType = Validate.Exists(
                await _userTypeRepo.GetById(-2).FirstOrDefaultAsync(),
                -2
            );

            var user = dto.Adapt<User>();
            user.Username = usernameNormalized;
            user.Email = emailInput;
            user.UserTypeId = userType.Id;

            //Hash password before saving
            user.HashedPassword = _passwordHasher.HashPassword(user, dto.Password);
            
            await _userRepo.AddAsync(user, userType.Id);
            await _userRepo.CommitAsync();

            var outDto = user.Adapt<UserListMessage>();
            outDto.UserTypeId = user.UserTypeId;
            outDto.UserRole = userType.Role;

            return outDto;
        }

        public async Task<UserListMessage> LoginUserAsync(LoginUserMessage dto)
        {
            Validate.ValidateModel(dto);

            var input = dto.UsernameOrEmail.Trim();
            var password = dto.Password.Trim();

            var users = _userRepo.GetAll();
            var user = users.FirstOrDefault(u =>
                string.Equals(u.Username, input, StringComparison.OrdinalIgnoreCase)
                || string.Equals(u.Email, input, StringComparison.OrdinalIgnoreCase)
            );

            if (user == null)
                throw new BadRequestException("Invalid username/email or password.");

            // Verify hashed password
            var result = _passwordHasher.VerifyHashedPassword(user, user.HashedPassword, password);
            if (result == PasswordVerificationResult.Failed)
                throw new BadRequestException("Invalid username/email or password.");

            // Block archived accounts
            if (user.IsArchived)
                throw new UnauthorizedAccessException("Archived accounts cannot log in.");

            // Block deactivated accounts
            if (!user.IsActive)
                throw new UnauthorizedAccessException("Deactivated accounts cannot log in.");

            // Build claims for JWT
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.UserType?.Role ?? "Normal")
            };

            // Null-safe config lookup
            var jwtKey = _config["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Borrow count from BorrowService
            var borrowCount = _borrowService.GetBorrowDetailsQuery()
                .Count(br => br.UserId == user.Id);

            // Map and enrich in one go
            var dtoOut = user.Adapt<UserListMessage>();
            dtoOut.UserRole = user.UserType?.Role ?? "Unknown";
            dtoOut.BorrowedBooksCount = borrowCount;
            dtoOut.Token = tokenString;

            return dtoOut;
        }

        public IQueryable<UserListMessage> GetUserByIdQuery(int id)
        {
            Validate.Positive(id, nameof(id));

            return _userRepo.GetAll()
                .Include(u => u.UserType)
                .AsNoTracking()
                .Select(u => new UserListMessage
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    UserRole = u.UserType != null ? u.UserType.Role : "Unknown",
                });
        }

        public IQueryable<UserListMessage> GetAllUsersQuery()
        {
            return _userRepo.GetAll()
                .Include(u => u.UserType)
                .AsNoTracking()
                .Select(u => new UserListMessage
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    UserRole = u.UserType != null ? u.UserType.Role : "Unknown"
                });
        }

        public async Task<LogoutUserMessage> LogoutUserAsync(int userId, string token)
        {
            Validate.Positive(userId, nameof(userId));
            Validate.NotEmpty(token, nameof(token));

            var user = Validate.Exists(
                await _userRepo.GetById(userId).FirstOrDefaultAsync(),
                userId
            );
            return new LogoutUserMessage
            {
                Id = user.Id,
                Username = user.Username,
                LoggedOutAt = DateTime.Now,
                Status = "Success"
            };
        }

        public async Task<UserListMessage> DeactivateUserAsync(int id, int performedByUserId)
        {
            Validate.Positive(id, nameof(id));
            Validate.Positive(performedByUserId, nameof(performedByUserId));

            var user = Validate.Exists(
                await _userRepo.GetById(id).FirstOrDefaultAsync(),
                id
            );

            var hasActiveBorrows = _borrowService.GetBorrowDetailsQuery()
                .Any(br => br.UserId == user.Id && br.ReturnDate == null);

            if (hasActiveBorrows)
                throw new InvalidOperationException("User has active borrowed books. Return them before deactivation.");

            await _userRepo.DeactivateAsync(user, performedByUserId);
            await _userRepo.CommitAsync();

            var dto = user.Adapt<UserListMessage>();
            dto.UserRole = user.UserType?.Role ?? "Unknown";

            return dto;
        }

        public async Task<UserListMessage> ReactivateUserAsync(int id, int performedByUserId)
        {
            Validate.Positive(id, nameof(id));
            Validate.Positive(performedByUserId, nameof(performedByUserId));

            var user = Validate.Exists(
                await _userRepo.GetById(id).FirstOrDefaultAsync(),
                id
            );

            if (user.IsArchived)
                throw new InvalidOperationException("Archived users cannot be reactivated.");

            await _userRepo.ReactivateAsync(user, performedByUserId);
            await _userRepo.CommitAsync();

            var dto = user.Adapt<UserListMessage>();
            dto.UserRole = user.UserType?.Role ?? "Unknown";

            return dto;
        }

        public async Task<UserListMessage> ArchiveUserAsync(int id, int performedByUserId)
        {
            Validate.Positive(id, nameof(id));
            Validate.Positive(performedByUserId, nameof(performedByUserId));

            var user = Validate.Exists(
                await _userRepo.GetById(id).FirstOrDefaultAsync(),
                id
            );

            await _userRepo.ArchiveAsync(user, performedByUserId);
            await _userRepo.CommitAsync();

            var dto = user.Adapt<UserListMessage>();
            dto.UserRole = user.UserType?.Role ?? "Unknown";

            return dto;
        }
    }
}