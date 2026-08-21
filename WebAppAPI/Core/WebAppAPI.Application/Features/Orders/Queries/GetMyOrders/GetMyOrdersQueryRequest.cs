using MediatR;

namespace WebAppAPI.Application.Features.Orders.Queries.GetMyOrders
{
    public sealed class GetMyOrdersQueryRequest : IRequest<GetMyOrdersQueryResponse>
    {
        public int Page { get; init; } = 0;
        public int Size { get; init; } = 5;
    }
}
