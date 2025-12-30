using Library.Common.RabbitMqMessages.UserMessages;

namespace Library.UserAPI.Interfaces
{
    public interface IUserService
    {
        Task<UserListMessage> RegisterUserAsync(RegisterUserMessage dto);
        Task<UserListMessage> LoginUserAsync(LoginMessage dto);
        IQueryable<UserListMessage> GetUserByIdQuery(int id);
        IQueryable<UserListMessage> GetAllUsersQuery();
        Task<UserListMessage> ArchiveUserAsync(int id, int performedByUserId);
    }
}
