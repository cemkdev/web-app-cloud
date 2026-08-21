namespace WebAppAPI.Application.Features.Orders.Queries.GetMyOrders
{
    public sealed class GetMyOrdersDto
    {
        public required int TotalOrderCount { get; init; }
        public required IReadOnlyList<MyOrderListItemDto> Orders { get; init; }
    }
}
