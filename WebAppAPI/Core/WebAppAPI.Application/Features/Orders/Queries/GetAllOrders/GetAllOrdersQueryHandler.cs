using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Orders.Queries.GetAllOrders
{
    public sealed class GetAllOrdersQueryHandler(IOrderService orderService) : IRequestHandler<GetAllOrdersQueryRequest, GetAllOrdersQueryResponse>
    {
        public async Task<GetAllOrdersQueryResponse> Handle(GetAllOrdersQueryRequest request, CancellationToken cancellationToken)
        {
            GetAllOrdersDto result = await orderService.GetAllOrdersAsync(
                request.Page,
                request.Size,
                cancellationToken);

            return new GetAllOrdersQueryResponse
            {
                TotalOrderCount = result.TotalOrderCount,
                Orders = result.Orders
            };
        }
    }
}
