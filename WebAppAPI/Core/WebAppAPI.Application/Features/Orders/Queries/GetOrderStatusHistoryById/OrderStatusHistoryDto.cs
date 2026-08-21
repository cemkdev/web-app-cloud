namespace WebAppAPI.Application.Features.Orders.Queries.GetOrderStatusHistoryById
{
    public sealed class OrderStatusHistoryDto
    {
        public required int CurrentStatusId { get; init; }
        public required IReadOnlyList<OrderStatusHistoryEntryDto> History { get; init; }
    }
}
