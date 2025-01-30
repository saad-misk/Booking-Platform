using System.Linq.Expressions;

namespace BookingPlatform.Domain.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync<TKey>(
            TKey id,
            CancellationToken cancellationToken = default
        );

        Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            IEnumerable<Expression<Func<T, object>>>? includes = null,
            int? skip = null,
            int? take = null,
            CancellationToken cancellationToken = default
        );

        Task AddAsync(
            T entity,
            CancellationToken cancellationToken = default
        );

        void Update(T entity);
        
        Task<bool> DeleteAsync<TKey>(
            TKey id,
            CancellationToken cancellationToken = default
        );
    }
}