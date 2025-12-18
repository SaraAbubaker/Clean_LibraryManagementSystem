
using Library.Shared.DTOs.User;

namespace Library.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserListDto> RegisterUserAsync(RegisterUserDto dto);
        Task<UserListDto> LoginUserAsync(LoginDto dto);
        IQueryable<UserListDto> GetUserByIdQuery(int id);
        IQueryable<UserListDto> GetAllUsersQuery();
        Task<UserListDto> ArchiveUserAsync(int id, int performedByUserId);
    }
}
