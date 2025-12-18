using Library.Shared.DTOs.UserType;

namespace Library.Services.Interfaces
{
    public interface IUserTypeService
    {
        Task<UserTypeListDto> CreateUserTypeAsync(CreateUserTypeDto dto, int createdByUserId);
        IQueryable<UserTypeListDto> GetAllUserTypesQuery();
        IQueryable<UserTypeListDto> GetUserTypeByIdQuery(int id);
        Task<UserTypeListDto> UpdateUserTypeAsync(UpdateUserTypeDto dto, int userId, int userTypeId);
        Task<bool> ArchiveUserTypeAsync(int id, int archivedByUserId);
    }
}
