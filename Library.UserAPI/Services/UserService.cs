
using Microsoft.EntityFrameworkCore;
using Library.UserAPI.Repositories.UserRepo;
using Library.Shared.Exceptions;
using Library.Shared.Helpers;
using Mapster;
using Library.UserAPI.Models;
using Library.UserAPI.Interfaces;
using Library.UserAPI.Repositories.UserTypeRepo;
using Library.Common.RabbitMqMessages.UserMessages;

namespace Library.UserAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IUserTypeRepository _userTypeRepo;

        public UserService(
            IUserRepository userRepo,
            IUserTypeRepository userTypeRepo)
        {
            _userRepo = userRepo;
            _userTypeRepo = userTypeRepo;
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

            //Normal user
            var userType = Validate.Exists(
                _userTypeRepo.GetById(-2).FirstOrDefault(),
                -2
            );

            var user = dto.Adapt<User>();
            user.Username = usernameNormalized;
            user.Email = emailInput;
            user.UserTypeId = userType.Id;
            user.BorrowRecords = new List<BorrowRecord>();

            await _userRepo.AddAsync(user, userType.Id);
            await _userRepo.CommitAsync();

            var outDto = user.Adapt<UserListMessage>();
            outDto.UserTypeId = user.UserTypeId;
            outDto.UserRole = userType.Role;
            outDto.BorrowedBooksCount = 0;

            return outDto;
        }

        public async Task<UserListMessage> LoginUserAsync(LoginMessage dto)
        {
            Validate.ValidateModel(dto);

            var input = dto.UsernameOrEmail.Trim();
            var password = dto.Password.Trim();

            var users = _userRepo.GetAll();
            var user = users.FirstOrDefault(u =>
                string.Equals(u.Username, input, StringComparison.OrdinalIgnoreCase)
                || string.Equals(u.Email, input, StringComparison.OrdinalIgnoreCase)
            );

            if (user == null || user.Password != password)
                throw new BadRequestException("Invalid username/email or password.");

            var result = user.Adapt<UserListMessage>();
            result.BorrowedBooksCount = user.BorrowRecords?.Count ?? 0;

            return result;
        }

        public IQueryable<UserListMessage> GetUserByIdQuery(int id)
        {
            Validate.Positive(id, nameof(id));

            return _userRepo.GetAll()
                .Include(u => u.UserType)
                .Include(u => u.BorrowRecords)
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new UserListMessage
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    UserRole = u.UserType != null ? u.UserType.Role : "Unknown",
                    BorrowedBooksCount = u.BorrowRecords.Count()
                });
        }

        public IQueryable<UserListMessage> GetAllUsersQuery()
        {
            return _userRepo.GetAll()
                .Include(u => u.UserType)
                .Include(u => u.BorrowRecords)
                .AsNoTracking()
                .Select(u => new UserListMessage
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    UserRole = u.UserType != null ? u.UserType.Role : "Unknown",
                    BorrowedBooksCount = u.BorrowRecords.Count()
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

            if (user.BorrowRecords != null && user.BorrowRecords.Any(br => br.ReturnDate == null))
                throw new InvalidOperationException("User has active borrowed books. Return them before deleting.");

            await _userRepo.ArchiveAsync(user, performedByUserId);
            await _userRepo.CommitAsync();

            var dto = user.Adapt<UserListMessage>();
            dto.BorrowedBooksCount = user.BorrowRecords?.Count ?? 0;

            return dto;
        }

    }
}
