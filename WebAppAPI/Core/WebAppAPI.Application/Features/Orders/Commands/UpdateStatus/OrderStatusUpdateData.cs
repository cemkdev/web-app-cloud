namespace WebAppAPI.Application.Features.Orders.Commands.UpdateStatus
{
    public sealed class OrderStatusUpdateData
    {
        public Guid OrderId { get; init; }
        public required string OrderCode { get; init; }
        public int StatusId { get; init; }
        public string? Recipient { get; init; }
        public required string FirstName { get; init; }
    }
}
