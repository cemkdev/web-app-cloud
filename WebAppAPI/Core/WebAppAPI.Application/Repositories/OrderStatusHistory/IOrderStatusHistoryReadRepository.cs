using E = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IOrderStatusHistoryReadRepository : IReadRepository<E.OrderStatusHistory>
    {
        Task<List<E.OrderStatusHistory>> GetByOrderIdAsync(Guid orderId, bool tracking = false);
    }
}
