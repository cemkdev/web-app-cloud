using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Features.Orders.Commands.UpdateStatus;
using WebAppAPI.Application.Features.Orders.Queries.GetAllOrders;
using WebAppAPI.Application.Features.Orders.Queries.GetMyOrderById;
using WebAppAPI.Application.Features.Orders.Queries.GetMyOrders;
using WebAppAPI.Application.Features.Orders.Queries.GetOrderById;
using WebAppAPI.Application.Features.Orders.Queries.GetOrderCustomerById;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public class OrderReadRepository(WebAppAPIDbContext context)
        : ReadRepository<Entities.Order>(context), IOrderReadRepository
    {
        public async Task<GetAllOrdersDto> GetPagedAsync(int page, int size, CancellationToken cancellationToken)
        {
            IQueryable<Entities.Order> query = Query(tracking: false);

            int totalOrderCount = await query.CountAsync(cancellationToken);

            List<OrderListItemDto> orders = await query
                .OrderByDescending(order => order.DateCreated)
                .ThenBy(order => order.Id)
                .Skip(page * size)
                .Take(size)
                .Select(order => new OrderListItemDto
                {
                    Id = order.Id,
                    OrderCode = order.OrderCode,
                    CustomerName = order.Basket.User.FullName,
                    TotalPrice = 0,
                    DateCreated = order.DateCreated,
                    StatusId = order.StatusId
                })
                .ToListAsync(cancellationToken);

            return new GetAllOrdersDto
            {
                TotalOrderCount = totalOrderCount,
                Orders = orders
            };
        }

        public async Task<GetMyOrdersDto> GetPagedByUserIdAsync(string userId, int page, int size, CancellationToken cancellationToken)
        {
            IQueryable<Entities.Order> query = Query(tracking: false).Where(order => order.Basket.UserId == userId);

            int totalOrderCount = await query.CountAsync(cancellationToken);

            List<MyOrderListItemDto> orders = await query
                .OrderByDescending(order => order.DateCreated)
                .ThenBy(order => order.Id)
                .Skip(page * size)
                .Take(size)
                .Select(order => new MyOrderListItemDto
                {
                    Id = order.Id,
                    OrderCode = order.OrderCode,
                    TotalPrice = 0,
                    DateCreated = order.DateCreated,
                    StatusId = order.StatusId
                })
                .ToListAsync(cancellationToken);

            return new GetMyOrdersDto
            {
                TotalOrderCount = totalOrderCount,
                Orders = orders
            };
        }

        public Task<Entities.Order?> GetDetailByIdAsync(Guid id, bool tracking = false)
            => Query(tracking)
                .Include(o => o.Basket)
                    .ThenInclude(b => b.BasketItems)
                        .ThenInclude(bi => bi.Product)
                            .ThenInclude(p => p.ProductImageFiles)
                .FirstOrDefaultAsync(o => o.Id == id);

        public Task<GetOrderByIdDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken)
            => Query(tracking: false)
                .Where(order => order.Id == id)
                .Select(order => new GetOrderByIdDto
                {
                    Id = order.Id,
                    OrderCode = order.OrderCode,
                    Address = order.Address,
                    Description = order.Description,
                    DateCreated = order.DateCreated,
                    StatusId = order.StatusId
                })
                .FirstOrDefaultAsync(cancellationToken);

        public Task<GetOrderCustomerByIdDto?> GetCustomerByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
            => Query(tracking: false)
                .Where(order => order.Id == orderId)
                .Select(order => new GetOrderCustomerByIdDto
                {
                    FullName = order.Basket.User.FullName,
                    Email = order.Basket.User.Email,
                    PhoneNumber = order.Basket.User.PhoneNumber
                })
                .FirstOrDefaultAsync(cancellationToken);

        public Task<int?> GetStatusIdByIdAsync(Guid id, CancellationToken cancellationToken)
            => Query(tracking: false)
                .Where(order => order.Id == id)
                .Select(order => (int?)order.StatusId)
                .FirstOrDefaultAsync(cancellationToken);

        public Task<int?> GetStatusIdByIdAndUserIdAsync(Guid id, string userId, CancellationToken cancellationToken)
            => Query(tracking: false)
                .Where(order =>
                    order.Id == id &&
                    order.Basket.UserId == userId)
                .Select(order => (int?)order.StatusId)
                .FirstOrDefaultAsync(cancellationToken);

        public Task<GetMyOrderByIdDto?> GetDetailByIdAndUserIdAsync(Guid id, string userId, CancellationToken cancellationToken)
            => Query(tracking: false)
                .Where(order =>
                    order.Id == id &&
                    order.Basket.UserId == userId)
                .Select(order => new GetMyOrderByIdDto
                {
                    Id = order.Id,
                    OrderCode = order.OrderCode,
                    Address = order.Address,
                    Description = order.Description,
                    DateCreated = order.DateCreated,
                    StatusId = order.StatusId
                })
                .FirstOrDefaultAsync(cancellationToken);

        public Task<OrderStatusUpdateData?> GetOrderStatusUpdateDetailsAsync(Guid id, CancellationToken cancellationToken)
            => Query(tracking: false)
                .Where(order => order.Id == id)
                .Select(order => new OrderStatusUpdateData
                {
                    OrderId = order.Id,
                    OrderCode = order.OrderCode,
                    StatusId = order.StatusId,
                    Recipient = order.Basket.User.Email,
                    FirstName = order.Basket.User.FirstName
                })
                .FirstOrDefaultAsync(cancellationToken);
    }
}
