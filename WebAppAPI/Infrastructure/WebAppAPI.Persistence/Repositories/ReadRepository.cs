using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Domain.Entities.Common;
using WebAppAPI.Persistence.Contexts;

namespace WebAppAPI.Persistence.Repositories
{
    public class ReadRepository<T>(WebAppAPIDbContext context) : IReadRepository<T> where T : BaseEntity
    {
        protected DbSet<T> Set => context.Set<T>();

        protected IQueryable<T> Query(bool tracking = false)
        {
            IQueryable<T> query = Set;

            return tracking ? query : query.AsNoTracking();
        }

        public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool tracking = false)
            => Query(tracking)
                .FirstOrDefaultAsync(
                    entity => entity.Id == id,
                    cancellationToken);

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
            => Query(tracking: false)
                .AnyAsync(
                    entity => entity.Id == id,
                    cancellationToken);
    }
}
