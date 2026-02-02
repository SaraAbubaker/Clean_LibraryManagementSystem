using Library.Common.DTOs.UserApiDtos.UserTypeDtos;

namespace Library.User.Services.Interfaces
{
    public interface IUserTypeService
    {
        Task<UserTypeListMessage> CreateUserTypeAsync(CreateUserTypeMessage dto, int createdByUserId);
        IQueryable<UserTypeListMessage> GetAllUserTypesQuery();
        IQueryable<UserTypeListMessage> GetUserTypeByIdQuery(int id);
        Task<UserTypeListMessage> UpdateUserTypeAsync(UpdateUserTypeMessage dto, int userId, int userTypeId);
        Task<bool> ArchiveUserTypeAsync(int id, int archivedByUserId);
    }
}
