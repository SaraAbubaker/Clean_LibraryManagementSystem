using Library.User.Domain.Data;
using Microsoft.EntityFrameworkCore;
using Library.User.Entities;

namespace Library.User.Domain.Repositories.UserRepo
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<ApplicationUser> _users;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
            _users = _context.Users;
        }

        public IQueryable<ApplicationUser> GetAll()
        {
            // Only return non-archived users
            return _users.Where(u => !u.IsArchived);
        }

        public IQueryable<ApplicationUser> GetById(int id)
        {
            return _users.Where(u => u.Id == id);
        }

        public async Task AddAsync(ApplicationUser entity, int currentUserId)
        {
            entity.CreatedByUserId = currentUserId;
            entity.CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow);
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.UtcNow);
            entity.IsArchived = false;

            await _users.AddAsync(entity);
        }

        public async Task UpdateAsync(ApplicationUser entity, int currentUserId)
        {
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.UtcNow);

            _users.Update(entity);
        }

        public async Task ArchiveAsync(ApplicationUser entity, int currentUserId)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.IsArchived = true;
            entity.ArchivedByUserId = currentUserId;
            entity.ArchivedDate = DateOnly.FromDateTime(DateTime.UtcNow);
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.UtcNow);

            _users.Update(entity);
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}