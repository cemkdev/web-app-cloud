using MediatR;

namespace WebAppAPI.Application.Features.Orders.Queries.GetMyOrderStatusHistoryById
{
    public sealed class GetMyOrderStatusHistoryByIdQueryRequest : IRequest<GetMyOrderStatusHistoryByIdQueryResponse>
    {
        public required string OrderId { get; init; }
    }
}
