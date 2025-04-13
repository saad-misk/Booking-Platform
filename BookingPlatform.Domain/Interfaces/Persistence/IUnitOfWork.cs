using BookingPlatform.Domain.Interfaces.Repositories;

namespace BookingPlatform.Domain.Interfaces.Persistence
{
    public interface IUnitOfWork
    {
        IRepository<T> GetRepository<T>() where T : class;
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task<int> SaveAsync();
    }
}