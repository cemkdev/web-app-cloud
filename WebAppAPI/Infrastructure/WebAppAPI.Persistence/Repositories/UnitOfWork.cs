using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;

namespace WebAppAPI.Persistence.Repositories
{
    public sealed class UnitOfWork(WebAppAPIDbContext context) : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => context.SaveChangesAsync(cancellationToken);

        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await operation(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
