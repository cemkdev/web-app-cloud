namespace WebAppAPI.Application.Features.Orders.Queries.GetAllOrders
{
    public sealed class GetAllOrdersQueryResponse
    {
        public required int TotalOrderCount { get; init; }
        public required IReadOnlyList<OrderListItemDto> Orders { get; init; }
    }
}
