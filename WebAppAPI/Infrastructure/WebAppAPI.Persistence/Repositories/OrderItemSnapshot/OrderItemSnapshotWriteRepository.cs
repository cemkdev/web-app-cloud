using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public sealed class OrderItemSnapshotWriteRepository(WebAppAPIDbContext context)
        : WriteRepository<Entities.OrderItemSnapshot>(context), IOrderItemSnapshotWriteRepository
    {
        public async Task<int> MarkProductsAsDeletedAsync(
            IReadOnlyCollection<Guid> productIds,
            CancellationToken cancellationToken)
        {
            if (productIds.Count == 0)
                return 0;

            Guid[] ids = productIds.ToArray();
            DateTime updatedAt = DateTime.UtcNow;

            return await Set
                .Where(snapshot =>
                    ids.Contains(snapshot.ProductId) &&
                    !snapshot.IsProductDeleted)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            snapshot => snapshot.IsProductDeleted,
                            true)
                        .SetProperty(
                            snapshot => snapshot.DateUpdated,
                            updatedAt),
                    cancellationToken);
        }
    }
}
