using System.Text.Json;
using WebAppAPI.Application.Abstractions.Messaging;
using WebAppAPI.Application.Abstractions.Messaging.Messages;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Infrastructure.Messaging.Handlers
{
    public sealed class OrderStatusUpdateMailMessageHandler(IMailService mailService) : IOutboxMessageHandler
    {
        public string MessageType => OutboxMessageTypes.OrderStatusUpdateMail;

        public async Task HandleAsync(string payload, CancellationToken cancellationToken)
        {
            OrderStatusUpdateMailMessage message = JsonSerializer.Deserialize<OrderStatusUpdateMailMessage>(payload)
                ?? throw new InvalidOperationException("Order status update mail outbox payload could not be deserialized.");

            await mailService.SendOrderStatusUpdateMailAsync(
                message.Recipient,
                message.OrderCode,
                message.NewStatus,
                message.StatusChangedDate,
                message.FirstName);
        }
    }
}