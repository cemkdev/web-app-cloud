using WebAppAPI.Domain.Enums;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IOrderWriteRepository : IWriteRepository<Entities.Order>
    {
        Task<bool> TryUpdateStatusAsync(
            Guid orderId,
            OrderStatusEnum expectedStatus,
            OrderStatusEnum newStatus,
            CancellationToken cancellationToken);
    }
}
