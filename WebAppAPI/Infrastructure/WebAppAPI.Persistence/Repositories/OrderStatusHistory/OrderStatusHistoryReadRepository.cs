using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using E = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public class OrderStatusHistoryReadRepository(WebAppAPIDbContext context)
        : ReadRepository<E.OrderStatusHistory>(context), IOrderStatusHistoryReadRepository
    {
        public Task<List<E.OrderStatusHistory>> GetByOrderIdAsync(Guid orderId, bool tracking = false)
            => Query(tracking)
                .Where(os => os.OrderId == orderId)
                .ToListAsync();
    }
}
