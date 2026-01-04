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
        private readonly DbSet<UserType> _userTypes;

        public UserRepository(UserContext context)
        {
            _context = context;
            _users = _context.Users;
            _userTypes = _context.UserTypes;
        }

        // ---------------- USER METHODS ----------------
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

        // ---------------- USERTYPE METHODS ----------------
        public IQueryable<UserType> GetAllUserTypes()
        {
            return _userTypes.AsNoTracking();
        }

        public IQueryable<UserType> GetUserTypeById(int id)
        {
            return _userTypes.Where(ut => ut.Id == id);
        }

        public async Task AddAsync(UserType entity, int currentUserId)
        {
            entity.CreatedByUserId = currentUserId;
            entity.CreatedDate = DateOnly.FromDateTime(DateTime.Now);
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.Now);
            entity.IsArchived = false;

            await _userTypes.AddAsync(entity);
        }

        public async Task UpdateAsync(UserType entity, int currentUserId)
        {
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.Now);

            _userTypes.Update(entity);
        }

        public async Task ArchiveAsync(UserType entity, int currentUserId)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.IsArchived = true;
            entity.ArchivedByUserId = currentUserId;
            entity.ArchivedDate = DateOnly.FromDateTime(DateTime.Now);
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.Now);

            _userTypes.Update(entity);
        }

        // ---------------- COMMIT ----------------
        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
