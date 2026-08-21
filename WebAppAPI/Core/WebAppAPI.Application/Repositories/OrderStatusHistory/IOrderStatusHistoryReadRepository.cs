using WebAppAPI.Application.Features.Orders.Queries.GetOrderStatusHistoryById;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IOrderStatusHistoryReadRepository : IReadRepository<Entities.OrderStatusHistory>
    {
        Task<IReadOnlyList<OrderStatusHistoryEntryDto>> GetStatusHistoryByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
    }
}
