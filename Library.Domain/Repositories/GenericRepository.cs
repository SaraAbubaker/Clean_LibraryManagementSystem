using Library.Domain.Data;
using Library.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace Library.Domain.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : AuditBase
    {
        private readonly LibraryContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(LibraryContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>(); //Access to tables
        }

        //Methods from IGenericRepository<T>
        public IQueryable<T> GetAll()
        {
            IQueryable<T> query = _dbSet.AsQueryable();

            if (typeof(IArchivable).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !e.IsArchived);
            }

            return query;
        }

        public IQueryable<T> GetById(int id)
        {
            return _dbSet.Where(e => EF.Property<int>(e, "Id") == id);
        }

        public async Task AddAsync(T entity, int currentUserId)
        {
            entity.CreatedByUserId = currentUserId;
            entity.CreatedDate = DateOnly.FromDateTime(DateTime.Now);
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.Now);
            entity.IsArchived = false;

            await _dbSet.AddAsync(entity);
        }

        public async Task UpdateAsync(T entity, int currentUserId)
        {
            entity.LastModifiedByUserId = currentUserId;
            entity.LastModifiedDate = DateOnly.FromDateTime(DateTime.Now);

            _dbSet.Update(entity);
        }

        public async Task ArchiveAsync(T entity, int currentUserId)
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
