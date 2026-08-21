using WebAppAPI.Domain.Enums;

namespace WebAppAPI.Application.Abstractions.Messaging.Messages
{
    public sealed class OrderStatusUpdateMailMessage
    {
        public required string Recipient { get; init; }
        public required string OrderCode { get; init; }
        public required OrderStatusEnum NewStatus { get; init; }
        public required DateTime StatusChangedDate { get; init; }
        public required string FirstName { get; init; }
    }
}
