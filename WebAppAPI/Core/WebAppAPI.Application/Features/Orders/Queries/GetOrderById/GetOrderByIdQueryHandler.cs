using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Orders.Queries.GetOrderById
{
    public sealed class GetOrderByIdQueryHandler(IOrderService orderService) : IRequestHandler<GetOrderByIdQueryRequest, GetOrderByIdQueryResponse>
    {
        public async Task<GetOrderByIdQueryResponse> Handle(GetOrderByIdQueryRequest request, CancellationToken cancellationToken)
        {
            GetOrderByIdDto order = await orderService.GetOrderByIdAsync(request.Id, cancellationToken);

            return new GetOrderByIdQueryResponse
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                Address = order.Address,
                Description = order.Description,
                DateCreated = order.DateCreated,
                StatusId = order.StatusId,
                OrderBasketItems = order.OrderBasketItems
            };
        }
    }
}
