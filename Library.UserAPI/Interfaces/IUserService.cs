using Library.Common.RabbitMqMessages.UserMessages;

namespace Library.UserAPI.Interfaces
{
    public interface IUserService
    {
        Task<UserListMessage> RegisterUserAsync(RegisterUserMessage dto);
        Task<UserListMessage> LoginUserAsync(LoginUserMessage dto);
        IQueryable<UserListMessage> GetUserByIdQuery(int id);
        IQueryable<UserListMessage> GetAllUsersQuery();
        Task<LogoutUserMessage> LogoutUserAsync(int userId, string token);
        Task<UserListMessage> DeactivateUserAsync(int id, int performedByUserId);
    }
}
