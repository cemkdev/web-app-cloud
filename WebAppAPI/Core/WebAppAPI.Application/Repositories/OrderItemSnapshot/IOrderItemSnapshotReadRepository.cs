using WebAppAPI.Application.Features.Orders.Queries.GetMyOrderById;
using WebAppAPI.Application.Features.Orders.Queries.GetOrderById;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IOrderItemSnapshotReadRepository : IReadRepository<Entities.OrderItemSnapshot>
    {
        Task<IReadOnlyDictionary<Guid, float>> GetTotalPricesByOrderIdsAsync(
            IReadOnlyCollection<Guid> orderIds,
            CancellationToken cancellationToken);

        Task<IReadOnlyDictionary<Guid, OrderDetailItemDto>> GetDetailItemsByOrderIdAsync(
            Guid orderId,
            CancellationToken cancellationToken);

        Task<IReadOnlyDictionary<Guid, MyOrderDetailItemDto>> GetMyDetailItemsByOrderIdAsync(
            Guid orderId,
            CancellationToken cancellationToken);
    }
}
