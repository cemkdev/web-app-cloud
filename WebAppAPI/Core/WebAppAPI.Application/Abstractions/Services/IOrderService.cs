using WebAppAPI.Application.Features.Orders.Commands.CreateOrder;
using WebAppAPI.Application.Features.Orders.Queries.GetAllOrders;
using WebAppAPI.Application.Features.Orders.Queries.GetMyOrderById;
using WebAppAPI.Application.Features.Orders.Queries.GetMyOrders;
using WebAppAPI.Application.Features.Orders.Queries.GetMyOrderStatusHistoryById;
using WebAppAPI.Application.Features.Orders.Queries.GetOrderById;
using WebAppAPI.Application.Features.Orders.Queries.GetOrderCustomerById;
using WebAppAPI.Application.Features.Orders.Queries.GetOrderStatusHistoryById;
using WebAppAPI.Domain.Enums;

namespace WebAppAPI.Application.Abstractions.Services
{
    public interface IOrderService
    {
        Task<GetAllOrdersDto> GetAllOrdersAsync(int page, int size, CancellationToken cancellationToken);
        Task<GetMyOrdersDto> GetMyOrdersAsync(int page, int size, CancellationToken cancellationToken);
        Task<GetOrderByIdDto> GetOrderByIdAsync(string id, CancellationToken cancellationToken);
        Task<GetOrderCustomerByIdDto> GetOrderCustomerByIdAsync(string orderId, CancellationToken cancellationToken);
        Task<GetMyOrderByIdDto> GetMyOrderByIdAsync(string id, CancellationToken cancellationToken);
        Task<OrderStatusHistoryDto> GetOrderStatusHistoryByIdAsync(string orderId, CancellationToken cancellationToken);
        Task<MyOrderStatusHistoryDto> GetMyOrderStatusHistoryByIdAsync(string orderId, CancellationToken cancellationToken);
        Task CreateOrderAsync(OrderCreateDto createOrder, CancellationToken cancellationToken);
        Task UpdateOrderStatusAsync(string orderId, OrderStatusEnum newStatus, CancellationToken cancellationToken);
        Task DeleteOrderAsync(string id, CancellationToken cancellationToken);
        Task DeleteRangeOrderAsync(IEnumerable<string> ids, CancellationToken cancellationToken);
    }
}
