using WebAppAPI.Application.Features.Orders.Commands.UpdateStatus;
using WebAppAPI.Application.Features.Orders.Queries.GetAllOrders;
using WebAppAPI.Application.Features.Orders.Queries.GetMyOrderById;
using WebAppAPI.Application.Features.Orders.Queries.GetMyOrders;
using WebAppAPI.Application.Features.Orders.Queries.GetOrderById;
using WebAppAPI.Application.Features.Orders.Queries.GetOrderCustomerById;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IOrderReadRepository : IReadRepository<Entities.Order>
    {
        Task<GetAllOrdersDto> GetPagedAsync(int page, int size, CancellationToken cancellationToken);
        Task<GetMyOrdersDto> GetPagedByUserIdAsync(string userId, int page, int size, CancellationToken cancellationToken);
        Task<GetOrderByIdDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<GetOrderCustomerByIdDto?> GetCustomerByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
        Task<int?> GetStatusIdByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<int?> GetStatusIdByIdAndUserIdAsync(Guid id, string userId, CancellationToken cancellationToken);
        Task<GetMyOrderByIdDto?> GetDetailByIdAndUserIdAsync(Guid id, string userId, CancellationToken cancellationToken);
        Task<OrderStatusUpdateData?> GetOrderStatusUpdateDetailsAsync(Guid id, CancellationToken cancellationToken);
    }
}
