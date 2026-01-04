
using Library.UserAPI.Data;
using Library.UserAPI.Models;
using Library.UserAPI.Repositories.UserTypeRepo.Library.UserAPI.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Library.UserAPI.Repositories.UserTypeRepo
{
    public class UserTypeRepository : IUserTypeRepository
    {
        private readonly UserContext _context;
        private readonly DbSet<UserType> _dbSet;

        public UserTypeRepository(UserContext context)
        {
            _context = context;
            _dbSet = _context.UserTypes;
        }

        public IQueryable<UserType> GetAll() => _dbSet.AsQueryable();

        public IQueryable<UserType> GetById(int id) => _dbSet.Where(e => e.Id == id);

        public async Task AddAsync(UserType entity, int currentUserId)
        {
            entity.CreatedByUserId = currentUserId;
            entity.CreatedDate = DateOnly.FromDateTime(DateTime.Now);
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.Now);
            entity.IsArchived = false;

            await _dbSet.AddAsync(entity);
        }

        public async Task UpdateAsync(UserType entity, int currentUserId)
        {
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.Now);
            _dbSet.Update(entity);
        }

        public async Task ArchiveAsync(UserType entity, int currentUserId)
        {
            entity.IsArchived = true;
            entity.ArchivedByUserId = currentUserId;
            entity.ArchivedDate = DateOnly.FromDateTime(DateTime.Now);
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.Now);
            _dbSet.Update(entity);
        }

        public async Task CommitAsync() => await _context.SaveChangesAsync();
    }
}