using Library.Common.Base;
using Library.UserAPI.Data;
using Library.UserAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.UserAPI.Repositories.UserRepo
{
    public class UserRepository : IUserRepository
    {
        private readonly UserContext _context;
        private readonly DbSet<User> _dbSet;

        public UserRepository(UserContext context)
        {
            _context = context;
            _dbSet = _context.Users; // Access to Users table
        }

        public IQueryable<User> GetAll()
        {
            IQueryable<User> query = _dbSet.AsQueryable();

            if (typeof(IArchivable).IsAssignableFrom(typeof(User)))
            {
                query = query.Where(e => !e.IsArchived);
            }

            return query;
        }

        public IQueryable<User> GetById(int id)
        {
            return _dbSet.Where(e => e.Id == id);
        }

        public async Task AddAsync(User entity, int currentUserId)
        {
            entity.CreatedByUserId = currentUserId;
            entity.CreatedDate = DateOnly.FromDateTime(DateTime.Now);
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.Now);
            entity.IsArchived = false;

            await _dbSet.AddAsync(entity);
        }

        public async Task UpdateAsync(User entity, int currentUserId)
        {
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.Now);

            _dbSet.Update(entity);
        }

        public async Task ArchiveAsync(User entity, int currentUserId)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.IsArchived = true;
            entity.ArchivedByUserId = currentUserId;
            entity.ArchivedDate = DateOnly.FromDateTime(DateTime.Now);
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.Now);

            _dbSet.Update(entity);
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}