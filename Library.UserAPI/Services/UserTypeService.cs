using Library.Common.RabbitMqMessages.UserTypeMessages;
using Library.Shared.Helpers;
using Library.UserAPI.Interfaces;
using Library.UserAPI.Models;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Library.UserAPI.Services
{
    public class UserTypeService : IUserTypeService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UserTypeService(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<UserTypeListMessage> CreateUserTypeAsync(CreateUserTypeMessage dto, int createdByUserId)
        {
            Validate.ValidateModel(dto);
            Validate.Positive(createdByUserId, nameof(createdByUserId));

            var existingAdmin = await _roleManager.Roles
                .FirstOrDefaultAsync(r => r.Name!.ToLower() == "admin");

            // Prevents duplication
            Validate.NotNull(
                dto.Role.ToLower() == "admin" && existingAdmin != null ? null : new object(),
                "The Admin role already exists and cannot be duplicated."
            );


            var role = new ApplicationRole
            {
                Name = dto.Role,
                CreatedByUserId = createdByUserId,
                CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                LastModifiedByUserId = createdByUserId,
                LastModifiedDate = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to create role: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            return role.Adapt<UserTypeListMessage>();
        }

        public IQueryable<UserTypeListMessage> GetAllUserTypesQuery()
        {
            return _roleManager.Roles
                .AsNoTracking()
                .Select(r => new UserTypeListMessage
                {
                    Id = r.Id,
                    Role = r.Name ?? string.Empty
                });
        }

        public IQueryable<UserTypeListMessage> GetUserTypeByIdQuery(int id)
        {
            Validate.Positive(id, nameof(id));

            return _roleManager.Roles
                .Where(r => r.Id == id)
                .AsNoTracking()
                .Select(r => new UserTypeListMessage
                {
                    Id = r.Id,
                    Role = r.Name ?? string.Empty
                });
        }

        public async Task<UserTypeListMessage> UpdateUserTypeAsync(UpdateUserTypeMessage dto, int userId, int userTypeId)
        {
            Validate.ValidateModel(dto);
            Validate.Positive(userTypeId, nameof(userTypeId));
            Validate.Positive(userId, nameof(userId));

            var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Id == userTypeId)
                ?? throw new KeyNotFoundException($"Role {userTypeId} not found.");

            // Prevent renaming Admin role
            Validate.NotEmpty(dto.Role, nameof(dto.Role));
            if (role.Name!.ToLower() == "admin" && dto.Role.ToLower() != "admin")
            {
                throw new InvalidOperationException("The Admin role cannot be renamed or downgraded.");
            }

            role.Name = dto.Role;
            role.LastModifiedByUserId = userId;
            role.LastModifiedDate = DateOnly.FromDateTime(DateTime.UtcNow);

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to update role: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            return role.Adapt<UserTypeListMessage>();
        }

        public async Task<bool> ArchiveUserTypeAsync(int id, int archivedByUserId)
        {
            Validate.Positive(id, nameof(id));
            Validate.Positive(archivedByUserId, nameof(archivedByUserId));

            var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new KeyNotFoundException($"Role {id} not found.");

            // Prevent archiving Admin role
            Validate.NotNull(
                role.Name!.ToLower() == "admin" ? null : new object(),
                "The Admin role cannot be archived."
            );

            role.IsArchived = true;
            role.ArchivedByUserId = archivedByUserId;
            role.ArchivedDate = DateOnly.FromDateTime(DateTime.UtcNow);
            role.LastModifiedByUserId = archivedByUserId;
            role.LastModifiedDate = DateOnly.FromDateTime(DateTime.UtcNow);

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to archive role: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            return true;
        }
    }
}