using Library.Common.Base;
using Library.UserAPI.Data;
using Library.UserAPI.Models;
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

        public IQueryable<UserType> GetAll()
        {
            IQueryable<UserType> query = _dbSet.AsQueryable();

            // If UserType implements IArchivable, filter archived ones
            if (typeof(IArchivable).IsAssignableFrom(typeof(UserType)))
            {
                query = query.Where(e => !e.IsArchived);
            }

            return query;
        }

        public IQueryable<UserType> GetById(int id)
        {
            return _dbSet.Where(e => e.Id == id);
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}