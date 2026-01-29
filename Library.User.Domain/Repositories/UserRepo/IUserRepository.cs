using Library.User.Entities;

namespace Library.User.Domain.Repositories.UserRepo
{
    public interface IUserRepository
    {
        IQueryable<ApplicationUser> GetAll();
        IQueryable<ApplicationUser> GetById(int id);
        Task AddAsync(ApplicationUser entity, int performedByUserId);
        Task ArchiveAsync(ApplicationUser entity, int currentUserId);
        Task CommitAsync();
    }
}