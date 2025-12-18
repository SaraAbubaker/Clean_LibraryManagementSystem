
namespace Library.Domain.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        IQueryable<T> GetById(int id);
        Task AddAsync(T entity, int currentUserId);
        Task UpdateAsync(T entity, int currentUserId);
        Task ArchiveAsync(T entity, int currentUserId);
        Task CommitAsync();
    }
}
