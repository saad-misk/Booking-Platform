using System.Collections.Concurrent;
using System.Transactions;
using BookingPlatform.Domain.Interfaces.Persistence;
using BookingPlatform.Domain.Interfaces.Repositories;
using BookingPlatform.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace BookingPlatform.Infrastructure.Persistence.DBContext
{
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;
        private readonly ConcurrentDictionary<Type, object> _repositories = new();
        private readonly ILogger<UnitOfWork> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public UnitOfWork(AppDbContext context, ILogger<UnitOfWork> logger, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            return (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ =>
                new Repository<T>(
                    _context,
                    _loggerFactory.CreateLogger<Repository<T>>()
                )
            );
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
                throw new InvalidOperationException("A transaction is already active.");

            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                _transaction = await _context.Database.BeginTransactionAsync();
                _logger.LogInformation("Transaction started.");
            });
        }

        public async Task CommitAsync()
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                try
                {
                    await _context.SaveChangesAsync();
                    if (_transaction != null)
                        await _transaction.CommitAsync();
                    _logger.LogInformation("Transaction committed successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Commit failed.");
                    throw new TransactionException("Commit failed", ex);
                }
                finally
                {
                    await DisposeTransactionAsync();
                }
            });
        }

        public async Task RollbackAsync()
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                try
                {
                    if (_transaction != null)
                        await _transaction.RollbackAsync();
                    _logger.LogInformation("Transaction rolled back.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Rollback failed.");
                    throw new TransactionException("Rollback failed", ex);
                }
                finally
                {
                    await DisposeTransactionAsync();
                }
            });
        }

        public async Task<int> SaveAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch
            {
                if (_transaction != null) await RollbackAsync();
                throw;
            }
        }

        private async Task DisposeTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeTransactionAsync();
            await _context.DisposeAsync();
        }
    }
}