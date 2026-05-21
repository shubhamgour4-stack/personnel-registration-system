using Microsoft.EntityFrameworkCore;
using PRS.Core.Interfaces;
using PRS.Infrastructure.Data;
using System.Linq.Expressions;

namespace PRS.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        internal DbSet<T> dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            this.dbSet = context.Set<T>();
        }

        public async Task<T> GetByIdAsync(int id) => await dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> GetAllAsync() => await dbSet.ToListAsync();

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) 
            => await dbSet.Where(predicate).ToListAsync();

        public async Task AddAsync(T entity) => await dbSet.AddAsync(entity);

        public void Update(T entity)
        {
            dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
                dbSet.Attach(entity);
            dbSet.Remove(entity);
        }
    }
}