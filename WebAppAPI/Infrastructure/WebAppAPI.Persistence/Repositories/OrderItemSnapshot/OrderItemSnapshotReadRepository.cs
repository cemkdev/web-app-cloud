using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Features.Orders.Queries.GetMyOrderById;
using WebAppAPI.Application.Features.Orders.Queries.GetOrderById;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public class OrderItemSnapshotReadRepository(WebAppAPIDbContext context)
        : ReadRepository<Entities.OrderItemSnapshot>(context), IOrderItemSnapshotReadRepository
    {
        public async Task<IReadOnlyDictionary<Guid, float>> GetTotalPricesByOrderIdsAsync(
            IReadOnlyCollection<Guid> orderIds,
            CancellationToken cancellationToken)
        {
            if (orderIds.Count == 0)
                return new Dictionary<Guid, float>();

            Guid[] ids = orderIds.ToArray();

            return await Query(tracking: false)
                .Where(snapshot => ids.Contains(snapshot.OrderId))
                .GroupBy(snapshot => snapshot.OrderId)
                .Select(group => new
                {
                    OrderId = group.Key,
                    TotalPrice = group.Sum(snapshot => snapshot.UnitPrice * snapshot.Quantity)
                })
                .ToDictionaryAsync(
                    item => item.OrderId,
                    item => item.TotalPrice,
                    cancellationToken);
        }

        public async Task<IReadOnlyDictionary<Guid, OrderDetailItemDto>> GetDetailItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
            => await Query(tracking: false)
                .Where(snapshot => snapshot.OrderId == orderId)
                .OrderBy(snapshot => snapshot.DateCreated)
                .ThenBy(snapshot => snapshot.Id)
                .Select(snapshot => new
                {
                    snapshot.ProductId,
                    Item = new OrderDetailItemDto
                    {
                        Name = snapshot.Name,
                        Title = snapshot.Title,
                        Description = snapshot.Description,
                        Price = snapshot.UnitPrice,
                        Quantity = snapshot.Quantity,
                        Rating = snapshot.Rating,
                        IsProductDeleted = snapshot.IsProductDeleted
                    }
                })
                .ToDictionaryAsync(
                    item => item.ProductId,
                    item => item.Item,
                    cancellationToken);

        public async Task<IReadOnlyDictionary<Guid, MyOrderDetailItemDto>> GetMyDetailItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
            => await Query(tracking: false)
                .Where(snapshot => snapshot.OrderId == orderId)
                .OrderBy(snapshot => snapshot.DateCreated)
                .ThenBy(snapshot => snapshot.Id)
                .Select(snapshot => new
                {
                    snapshot.ProductId,
                    Item = new MyOrderDetailItemDto
                    {
                        Name = snapshot.Name,
                        Title = snapshot.Title,
                        Description = snapshot.Description,
                        Price = snapshot.UnitPrice,
                        Quantity = snapshot.Quantity,
                        Rating = snapshot.Rating,
                        IsProductDeleted = snapshot.IsProductDeleted
                    }
                })
                .ToDictionaryAsync(
                    item => item.ProductId,
                    item => item.Item,
                    cancellationToken);
    }
}
