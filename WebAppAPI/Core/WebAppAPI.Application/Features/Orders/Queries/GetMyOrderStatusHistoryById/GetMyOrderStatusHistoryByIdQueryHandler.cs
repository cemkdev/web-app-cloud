using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Orders.Queries.GetMyOrderStatusHistoryById
{
    public sealed class GetMyOrderStatusHistoryByIdQueryHandler(IOrderService orderService)
        : IRequestHandler<GetMyOrderStatusHistoryByIdQueryRequest, GetMyOrderStatusHistoryByIdQueryResponse>
    {
        public async Task<GetMyOrderStatusHistoryByIdQueryResponse> Handle(GetMyOrderStatusHistoryByIdQueryRequest request, CancellationToken cancellationToken)
        {
            MyOrderStatusHistoryDto orderStatusHistory = await orderService.GetMyOrderStatusHistoryByIdAsync(request.OrderId, cancellationToken);

            return new GetMyOrderStatusHistoryByIdQueryResponse
            {
                CurrentStatusId = orderStatusHistory.CurrentStatusId,
                History = orderStatusHistory.History
            };
        }
    }
}
