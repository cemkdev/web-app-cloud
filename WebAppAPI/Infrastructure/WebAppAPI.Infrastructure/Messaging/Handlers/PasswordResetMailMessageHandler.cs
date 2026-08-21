using System.Text.Json;
using WebAppAPI.Application.Abstractions.Messaging;
using WebAppAPI.Application.Abstractions.Messaging.Messages;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Infrastructure.Messaging.Handlers
{
    public sealed class PasswordResetMailMessageHandler(IMailService mailService) : IOutboxMessageHandler
    {
        public string MessageType => OutboxMessageTypes.PasswordResetMail;

        public async Task HandleAsync(string payload, CancellationToken cancellationToken)
        {
            PasswordResetMailMessage message = JsonSerializer.Deserialize<PasswordResetMailMessage>(payload)
                ?? throw new InvalidOperationException("Password reset mail outbox payload could not be deserialized.");

            await mailService.SendPasswordResetMailAsync(
                message.Recipient,
                message.UserId,
                message.FirstName,
                message.ResetToken);
        }
    }
}
