using System.Linq.Expressions;
using BookingPlatform.Domain.Interfaces.Repositories;
using BookingPlatform.Infrastructure.Persistence.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingPlatform.Infrastructure.Persistence.Repositories
{
    public class Repository<T>(AppDbContext context, ILogger<Repository<T>> logger)
        : IRepository<T> where T : class
    {
        protected readonly DbSet<T> _dbSet = context.Set<T>();
        protected readonly AppDbContext _context = context;
        protected readonly ILogger<Repository<T>> _logger = logger;

        public async Task<T?> GetByIdAsync<TKey>(
            TKey id,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                return await _dbSet.FindAsync([id], cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch {Entity} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            IEnumerable<Expression<Func<T, object>>>? includes = null,
            int? skip = null,
            int? take = null,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var query = _dbSet.AsQueryable();

                if (includes != null)
                    query = includes.Aggregate(query, (current, include) => current.Include(include));

                if (filter != null)
                    query = query.Where(filter);

                if (orderBy != null)
                    query = orderBy(query);

                if (skip.HasValue)
                    query = query.Skip(skip.Value);

                if (take.HasValue)
                    query = query.Take(take.Value);

                return await query.ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to query {Entities}", typeof(T).Name);
                throw;
            }
        }

        public async Task<int> CountAsync(
            Expression<Func<T, bool>>? filter = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _dbSet.AsQueryable();
                if (filter != null)
                    query = query.Where(filter);
                
                return await query.CountAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to count {Entities}", typeof(T).Name);
                throw;
            }
        }

        public async Task AddAsync(
            T entity,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                await _dbSet.AddAsync(entity, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add {Entity}", typeof(T).Name);
                throw;
            }
        }

        public void Update(T entity)
        {
            try
            {
                _dbSet.Update(entity);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update {Entity}", typeof(T).Name);
                throw;
            }
        }

        public async Task<bool> DeleteAsync<TKey>(
            TKey id,
            CancellationToken cancellationToken = default
        )
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity is null) return false;

            try
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete {Entity} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }
    }
}