using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Orders.Queries.GetMyOrders
{
    public sealed class GetMyOrdersQueryHandler(IOrderService orderService) : IRequestHandler<GetMyOrdersQueryRequest, GetMyOrdersQueryResponse>
    {
        public async Task<GetMyOrdersQueryResponse> Handle(GetMyOrdersQueryRequest request, CancellationToken cancellationToken)
        {
            GetMyOrdersDto result = await orderService.GetMyOrdersAsync(
                request.Page,
                request.Size,
                cancellationToken);

            return new GetMyOrdersQueryResponse
            {
                TotalOrderCount = result.TotalOrderCount,
                Orders = result.Orders
            };
        }
    }
}
