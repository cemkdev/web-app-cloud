using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Domain.Enums;
using WebAppAPI.Persistence.Contexts;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public sealed class OrderWriteRepository(WebAppAPIDbContext context) : WriteRepository<Entities.Order>(context), IOrderWriteRepository
    {
        public async Task<bool> TryUpdateStatusAsync(Guid orderId, OrderStatusEnum expectedStatus, OrderStatusEnum newStatus, CancellationToken cancellationToken)
        {
            DateTime updatedAt = DateTime.UtcNow;

            int affectedRows = await Set
                .Where(order =>
                    order.Id == orderId &&
                    order.StatusId == (int)expectedStatus)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            order => order.StatusId,
                            (int)newStatus)
                        .SetProperty(
                            order => order.DateUpdated,
                            updatedAt),
                    cancellationToken);

            return affectedRows == 1;
        }
    }
}
