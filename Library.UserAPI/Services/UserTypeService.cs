using Library.Common.RabbitMqMessages.UserTypeMessages;
using Library.Shared.Helpers;
using Library.UserAPI.Interfaces;
using Library.UserAPI.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Library.UserAPI.Services
{
    public class UserTypeService : IUserTypeService
    {
        private readonly IUserTypeRepository _userTypeRepo;

        public UserTypeService(IUserTypeRepository userTypeRepo)
        {
            _userTypeRepo = userTypeRepo;
        }

        public async Task<UserTypeListMessage> CreateUserTypeAsync(CreateUserTypeMessage dto, int createdByUserId)
        {
            Validate.ValidateModel(dto);
            Validate.Positive(createdByUserId, nameof(createdByUserId));

            var existingAdmin = await _userTypeRepo.GetAll()
                .FirstOrDefaultAsync(ut => ut.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase));

            Validate.NotNull(
                dto.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && existingAdmin != null ? null : new object(),
                "The Admin role already exists and cannot be duplicated."
            );

            var userType = dto.Adapt<UserType>();
            await _userTypeRepo.AddAsync(userType, createdByUserId);
            await _userTypeRepo.CommitAsync();

            return userType.Adapt<UserTypeListMessage>();
        }

        public IQueryable<UserTypeListMessage> GetAllUserTypesQuery()
        {
            return _userTypeRepo.GetAll()
                .AsNoTracking()
                .Select(ut => new UserTypeListMessage
                {
                    Id = ut.Id,
                    Role = ut.Role
                });
        }

        public IQueryable<UserTypeListMessage> GetUserTypeByIdQuery(int id)
        {
            Validate.Positive(id, nameof(id));

            return _userTypeRepo.GetById(id)
                .AsNoTracking()
                .Select(ut => new UserTypeListMessage
                {
                    Id = ut.Id,
                    Role = ut.Role
                });
        }

        public async Task<UserTypeListMessage> UpdateUserTypeAsync(UpdateUserTypeMessage dto, int userId, int userTypeId)
        {
            Validate.ValidateModel(dto);
            Validate.Positive(userTypeId, nameof(userTypeId));
            Validate.Positive(userId, nameof(userId));

            var userType = Validate.Exists(
                await _userTypeRepo.GetById(userTypeId).FirstOrDefaultAsync(),
                userTypeId
            );

            // Prevent renaming Admin role
            Validate.NotEmpty(dto.Role, nameof(dto.Role));
            if (userType.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) &&
                !dto.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The Admin role cannot be renamed or downgraded.");
            }

            userType.Role = dto.Role;
            await _userTypeRepo.UpdateAsync(userType, userId);
            await _userTypeRepo.CommitAsync();

            return userType.Adapt<UserTypeListMessage>();
        }

        public async Task<bool> ArchiveUserTypeAsync(int id, int archivedByUserId)
        {
            Validate.Positive(id, nameof(id));
            Validate.Positive(archivedByUserId, nameof(archivedByUserId));

            var userType = Validate.Exists(
                await _userTypeRepo.GetById(id).FirstOrDefaultAsync(),
                id
            );

            // Prevent archiving Admin role
            Validate.NotNull(
                userType.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? null : new object(),
                "The Admin role cannot be archived."
            );

            await _userTypeRepo.ArchiveAsync(userType, archivedByUserId);
            await _userTypeRepo.CommitAsync();

            return true;
        }
    }
}