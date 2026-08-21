using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IOrderItemSnapshotWriteRepository : IWriteRepository<Entities.OrderItemSnapshot>
    {
        Task<int> MarkProductsAsDeletedAsync(
            IReadOnlyCollection<Guid> productIds,
            CancellationToken cancellationToken);
    }
}
