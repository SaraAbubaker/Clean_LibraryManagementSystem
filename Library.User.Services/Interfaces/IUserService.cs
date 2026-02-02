using Library.Common.DTOs.UserApiDtos.UserDtos;

namespace Library.User.Services.Interfaces
{
    public interface IUserService
    {
        Task<LoginUserResponseMessage> RegisterUserAsync(RegisterUserMessage dto);
        Task<LoginUserResponseMessage> LoginUserAsync(LoginUserMessage dto);
        IQueryable<UserListMessage> GetUserByIdQuery(int id);
        IQueryable<UserListMessage> GetAllUsersQuery();
        Task<LogoutUserMessage> LogoutUserAsync(int userId, string token);
        Task<UserListMessage> DeactivateUserAsync(int id, int performedByUserId);
        Task<UserListMessage> ReactivateUserAsync(int id, int performedByUserId);
        Task<UserListMessage> ArchiveUserAsync(int id, int performedByUserId);
    }
}
