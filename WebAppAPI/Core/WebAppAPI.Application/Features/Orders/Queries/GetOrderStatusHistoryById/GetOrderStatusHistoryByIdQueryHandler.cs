using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Orders.Queries.GetOrderStatusHistoryById
{
    public sealed class GetOrderStatusHistoryByIdQueryHandler(IOrderService orderService)
        : IRequestHandler<GetOrderStatusHistoryByIdQueryRequest, GetOrderStatusHistoryByIdQueryResponse>
    {
        public async Task<GetOrderStatusHistoryByIdQueryResponse> Handle(GetOrderStatusHistoryByIdQueryRequest request, CancellationToken cancellationToken)
        {
            OrderStatusHistoryDto orderStatusHistory = await orderService.GetOrderStatusHistoryByIdAsync(request.OrderId, cancellationToken);

            return new GetOrderStatusHistoryByIdQueryResponse
            {
                CurrentStatusId = orderStatusHistory.CurrentStatusId,
                History = orderStatusHistory.History
            };
        }
    }
}
