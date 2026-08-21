using MediatR;

namespace WebAppAPI.Application.Features.Orders.Queries.GetOrderStatusHistoryById
{
    public sealed class GetOrderStatusHistoryByIdQueryRequest : IRequest<GetOrderStatusHistoryByIdQueryResponse>
    {
        public required string OrderId { get; init; }
    }
}
