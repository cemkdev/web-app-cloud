using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Features.Orders.Queries.GetOrderStatusHistoryById;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public class OrderStatusHistoryReadRepository(WebAppAPIDbContext context)
        : ReadRepository<Entities.OrderStatusHistory>(context), IOrderStatusHistoryReadRepository
    {
        public async Task<IReadOnlyList<OrderStatusHistoryEntryDto>> GetStatusHistoryByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
            => await Query(tracking: false)
                .Where(history => history.OrderId == orderId)
                .OrderBy(history => history.ChangedDate)
                .ThenBy(history => history.Id)
                .Select(history => new OrderStatusHistoryEntryDto
                {
                    NewStatusId = history.NewStatusId,
                    ChangedDate = history.ChangedDate
                })
                .ToListAsync(cancellationToken);
    }
}
