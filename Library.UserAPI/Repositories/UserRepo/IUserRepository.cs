using Library.UserAPI.Models;

namespace Library.UserAPI.Repositories.UserRepo
{
    public interface IUserRepository
    {
        IQueryable<User> GetAll();
        IQueryable<User> GetById(int id);
        Task AddAsync(User entity, int performedByUserId);
        Task ArchiveAsync(User entity, int performedByUserId);
        Task CommitAsync();
    }
}