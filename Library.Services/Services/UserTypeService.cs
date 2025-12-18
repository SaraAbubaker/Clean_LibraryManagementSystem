using Library.Domain.Repositories;
using Library.Entities.Models;
using Library.Services.Interfaces;
using Library.Shared.DTOs.UserType;
using Library.Shared.Helpers;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Library.Services.Services
{
    public class UserTypeService : IUserTypeService
    {
        private readonly IGenericRepository<UserType> _userTypeRepo;

        public UserTypeService(IGenericRepository<UserType> userTypeRepo)
        {
            _userTypeRepo = userTypeRepo;
        }

        //CRUD
        public async Task<UserTypeListDto> CreateUserTypeAsync(CreateUserTypeDto dto, int createdByUserId)
        {
            Validate.ValidateModel(dto);
            Validate.Positive(createdByUserId, nameof(createdByUserId));

            var userType = dto.Adapt<UserType>();

            await _userTypeRepo.AddAsync(userType, createdByUserId);
            await _userTypeRepo.CommitAsync();

            return userType.Adapt<UserTypeListDto>();
        }

        public IQueryable<UserTypeListDto> GetAllUserTypesQuery()
        {
            return _userTypeRepo.GetAll()
                .AsNoTracking()
                .Select(ut => new UserTypeListDto
                {
                    Id = ut.Id,
                    Role = ut.Role
                });
        }

        public IQueryable<UserTypeListDto> GetUserTypeByIdQuery(int id)
        {
            Validate.Positive(id, nameof(id));

            return _userTypeRepo.GetAll()
                .AsNoTracking()
                .Where(ut => ut.Id == id)
                .Select(ut => new UserTypeListDto
                {
                    Id = ut.Id,
                    Role = ut.Role
                });
        }

        public async Task<UserTypeListDto> UpdateUserTypeAsync(UpdateUserTypeDto dto, int userId, int userTypeId)
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

            return userType.Adapt<UserTypeListDto>();
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
