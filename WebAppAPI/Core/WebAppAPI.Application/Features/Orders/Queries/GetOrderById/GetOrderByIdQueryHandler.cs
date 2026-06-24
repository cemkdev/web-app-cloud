using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Orders.Queries.GetOrderById
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQueryRequest, GetOrderByIdQueryResponse>
    {
        readonly IOrderService _orderService;

        public GetOrderByIdQueryHandler(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<GetOrderByIdQueryResponse> Handle(GetOrderByIdQueryRequest request, CancellationToken cancellationToken)
        {
            var order = await _orderService.GetOrderByIdAsync(request.Id);

            return new()
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                Description = order.Description,
                Address = order.Address,
                DateCreated = order.DateCreated,
                StatusId = order.StatusId,
                OrderBasketItems = order.OrderBasketItems
            };
        }
    }
}
