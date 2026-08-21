namespace WebAppAPI.Application.Features.Orders.Queries.GetMyOrderStatusHistoryById
{
    public sealed class MyOrderStatusHistoryEntryDto
    {
        public required int NewStatusId { get; init; }
        public required DateTime ChangedDate { get; init; }
    }
}
