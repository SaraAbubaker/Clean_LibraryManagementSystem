
using Microsoft.EntityFrameworkCore;
using Library.Shared.Helpers;
using Mapster;
using Library.UserAPI.Interfaces;
using Library.UserAPI.Models;
using Library.Common.RabbitMqMessages.UserTypeMessages;
using Library.UserAPI.Repositories.UserTypeRepo;

namespace Library.UserAPI.Services
{
    public class UserTypeService : IUserTypeService
    {
        private readonly IUserTypeRepository _userTypeRepo;

        public UserTypeService(IUserTypeRepository userTypeRepo)
        {
            _userTypeRepo = userTypeRepo;
        }

        //CRUD
        public async Task<UserTypeListMessage> CreateUserTypeAsync(CreateUserTypeMessage dto, int createdByUserId)
        {
            Validate.ValidateModel(dto);
            Validate.Positive(createdByUserId, nameof(createdByUserId));

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

            return _userTypeRepo.GetAll()
                .AsNoTracking()
                .Where(ut => ut.Id == id)
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

            await _userTypeRepo.ArchiveAsync(userType, archivedByUserId);
            await _userTypeRepo.CommitAsync();

            return true;
        }
    }
}
