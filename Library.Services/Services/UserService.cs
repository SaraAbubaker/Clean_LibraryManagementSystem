using Library.Domain.Repositories;
using Library.Entities.Models;
using Library.Services.Interfaces;
using Library.Shared.DTOs.User;
using Library.Shared.Exceptions;
using Library.Shared.Helpers;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Library.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<UserType> _userTypeRepo;

        public UserService(
            IGenericRepository<User> userRepo,
            IGenericRepository<UserType> userTypeRepo)
        {
            _userRepo = userRepo;
            _userTypeRepo = userTypeRepo;
        }


        public async Task<UserListDto> RegisterUserAsync(RegisterUserDto dto)
        {
            Validate.ValidateModel(dto);

            var usernameNormalized = dto.Username.Trim();
            var emailInput = dto.Email.Trim();

            var users = _userRepo.GetAll();

            if (users.Any(u => string.Equals(u.Username, usernameNormalized, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Username already taken.");

            if (users.Any(u => string.Equals(u.Email, emailInput, StringComparison.OrdinalIgnoreCase)))
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

            var outDto = user.Adapt<UserListDto>();
            outDto.UserTypeId = user.UserTypeId;
            outDto.UserRole = userType.Role;
            outDto.BorrowedBooksCount = 0;

            return outDto;
        }

        public async Task<UserListDto> LoginUserAsync(LoginDto dto)
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

            var result = user.Adapt<UserListDto>();
            result.BorrowedBooksCount = user.BorrowRecords?.Count ?? 0;

            return result;
        }

        public IQueryable<UserListDto> GetUserByIdQuery(int id)
        {
            Validate.Positive(id, nameof(id));

            return _userRepo.GetAll()
                .Include(u => u.UserType)
                .Include(u => u.BorrowRecords)
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    UserRole = u.UserType != null ? u.UserType.Role : "Unknown",
                    BorrowedBooksCount = u.BorrowRecords.Count()
                });
        }

        public IQueryable<UserListDto> GetAllUsersQuery()
        {
            return _userRepo.GetAll()
                .Include(u => u.UserType)
                .Include(u => u.BorrowRecords)
                .AsNoTracking()
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    UserRole = u.UserType != null ? u.UserType.Role : "Unknown",
                    BorrowedBooksCount = u.BorrowRecords.Count()
                });
        }

        public async Task<UserListDto> ArchiveUserAsync(int id, int performedByUserId)
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

            var dto = user.Adapt<UserListDto>();
            dto.BorrowedBooksCount = user.BorrowRecords?.Count ?? 0;

            return dto;
        }

    }
}
