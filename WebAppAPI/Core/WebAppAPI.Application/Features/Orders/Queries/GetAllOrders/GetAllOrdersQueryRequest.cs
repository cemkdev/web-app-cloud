using MediatR;

namespace WebAppAPI.Application.Features.Orders.Queries.GetAllOrders
{
    public sealed class GetAllOrdersQueryRequest : IRequest<GetAllOrdersQueryResponse>
    {
        public int Page { get; init; } = 0;
        public int Size { get; init; } = 5;
    }
}
