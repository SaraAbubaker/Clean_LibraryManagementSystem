
using Library.UserAPI.Models;

namespace Library.UserAPI.Interfaces
{
    public interface IUserTypeRepository
    {
        IQueryable<UserType> GetAll();
        IQueryable<UserType> GetById(int id);
        Task AddAsync(UserType entity, int currentUserId);
        Task UpdateAsync(UserType entity, int currentUserId);
        Task ArchiveAsync(UserType entity, int currentUserId);
        Task CommitAsync();
    }
}
