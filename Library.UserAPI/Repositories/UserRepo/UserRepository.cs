using Library.Common.Base;
using Library.UserAPI.Data;
using Library.UserAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.UserAPI.Repositories.UserRepo
{
    public class UserRepository : IUserRepository
    {
        private readonly UserContext _context;
        private readonly DbSet<User> _users;

        public UserRepository(UserContext context)
        {
            _context = context;
            _users = _context.Users;
        }

        public IQueryable<User> GetAll()
        {
            IQueryable<User> query = _users.AsQueryable();

            if (typeof(IArchivable).IsAssignableFrom(typeof(User)))
            {
                query = query.Where(e => !e.IsArchived);
            }

            return query;
        }

        public IQueryable<User> GetById(int id)
        {
            return _users.Where(e => e.Id == id);
        }

        public async Task AddAsync(User entity, int currentUserId)
        {
            entity.CreatedByUserId = currentUserId;
            entity.CreatedDate = DateOnly.FromDateTime(DateTime.Now);
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.Now);
            entity.IsArchived = false;

            await _users.AddAsync(entity);
        }

        public async Task UpdateAsync(User entity, int currentUserId)
        {
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.Now);

            _users.Update(entity);
        }

        public async Task DeactivateAsync(User entity, int currentUserId)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.IsActive = false;
            entity.DeactivatedByUserId = currentUserId;
            entity.DeactivatedDate = DateOnly.FromDateTime(DateTime.Now);
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.Now);

            _users.Update(entity);
        }

        public async Task ReactivateAsync(User entity, int currentUserId)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.IsActive = true;
            entity.DeactivatedByUserId = null;
            entity.DeactivatedDate = null;
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.Now);

            _users.Update(entity);
        }

        public async Task ArchiveAsync(User entity, int currentUserId)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.IsArchived = true;
            entity.ArchivedByUserId = currentUserId;
            entity.ArchivedDate = DateOnly.FromDateTime(DateTime.Now);
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.Now);

            _users.Update(entity);
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
