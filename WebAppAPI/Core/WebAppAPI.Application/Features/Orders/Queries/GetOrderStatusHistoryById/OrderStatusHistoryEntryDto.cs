namespace WebAppAPI.Application.Features.Orders.Queries.GetOrderStatusHistoryById
{
    public sealed class OrderStatusHistoryEntryDto
    {
        public required int NewStatusId { get; init; }
        public required DateTime ChangedDate { get; init; }
    }
}
