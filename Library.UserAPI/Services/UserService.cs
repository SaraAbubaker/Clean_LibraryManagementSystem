using Library.Common.RabbitMqMessages.UserMessages;
using Library.Services.Interfaces;
using Library.Shared.Exceptions;
using Library.Shared.Helpers;
using Library.UserAPI.Interfaces;
using Library.UserAPI.Models;
using Library.UserAPI.Repositories.UserRepo;
using Library.UserAPI.Repositories.UserTypeRepo.Library.UserAPI.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Library.UserAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IUserTypeRepository _userTypeRepo;
        private readonly IBorrowService _borrowService;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(
            IUserRepository userRepo,
            IUserTypeRepository userTypeRepo,
            IBorrowService borrowService,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepo = userRepo;
            _userTypeRepo = userTypeRepo;
            _borrowService = borrowService;
            _passwordHasher = passwordHasher;
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
            user.Password = _passwordHasher.HashPassword(user, dto.Password);

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

            //Verify hashed password
            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            if (result == PasswordVerificationResult.Failed)
                throw new BadRequestException("Invalid username/email or password.");

            var dtoOut = user.Adapt<UserListMessage>();
            dtoOut.UserRole = user.UserType?.Role ?? "Unknown";

            //Use BorrowService to get borrow count
            var borrowCount = _borrowService.GetBorrowDetailsQuery()
                .Count(br => br.UserId == user.Id);

            dtoOut.BorrowedBooksCount = borrowCount;

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

        public async Task<UserListMessage> ArchiveUserAsync(int id, int performedByUserId)
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
                throw new InvalidOperationException("User has active borrowed books. Return them before deleting.");

            await _userRepo.ArchiveAsync(user, performedByUserId);
            await _userRepo.CommitAsync();

            var dto = user.Adapt<UserListMessage>();

            return dto;
        }
    }
}