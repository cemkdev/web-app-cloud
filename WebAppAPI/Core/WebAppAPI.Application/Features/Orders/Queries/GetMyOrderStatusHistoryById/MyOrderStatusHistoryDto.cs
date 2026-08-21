namespace WebAppAPI.Application.Features.Orders.Queries.GetMyOrderStatusHistoryById
{
    public sealed class MyOrderStatusHistoryDto
    {
        public required int CurrentStatusId { get; init; }
        public required IReadOnlyList<MyOrderStatusHistoryEntryDto> History { get; init; }
    }
}
