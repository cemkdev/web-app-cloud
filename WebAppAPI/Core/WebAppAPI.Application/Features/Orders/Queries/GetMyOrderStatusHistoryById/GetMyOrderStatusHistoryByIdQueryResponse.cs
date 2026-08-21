namespace WebAppAPI.Application.Features.Orders.Queries.GetMyOrderStatusHistoryById
{
    public sealed class GetMyOrderStatusHistoryByIdQueryResponse
    {
        public required int CurrentStatusId { get; init; }
        public required IReadOnlyList<MyOrderStatusHistoryEntryDto> History { get; init; }
    }
}
