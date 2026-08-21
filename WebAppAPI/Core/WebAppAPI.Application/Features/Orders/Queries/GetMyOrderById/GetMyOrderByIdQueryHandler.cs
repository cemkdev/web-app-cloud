using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Orders.Queries.GetMyOrderById
{
    public sealed class GetMyOrderByIdQueryHandler(IOrderService orderService)
        : IRequestHandler<GetMyOrderByIdQueryRequest, GetMyOrderByIdQueryResponse>
    {
        public async Task<GetMyOrderByIdQueryResponse> Handle(GetMyOrderByIdQueryRequest request, CancellationToken cancellationToken)
        {
            GetMyOrderByIdDto order = await orderService.GetMyOrderByIdAsync(request.Id, cancellationToken);

            return new GetMyOrderByIdQueryResponse
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
