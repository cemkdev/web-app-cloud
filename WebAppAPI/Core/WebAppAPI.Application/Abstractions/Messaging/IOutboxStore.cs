using WebAppAPI.Application.Abstractions.Messaging.Models;

namespace WebAppAPI.Application.Abstractions.Messaging
{
    public interface IOutboxStore
    {
        Task<IReadOnlyList<OutboxMessageEnvelope>> ClaimPendingAsync(
            int batchSize,
            TimeSpan lockDuration,
            CancellationToken cancellationToken);

        Task<bool> MarkProcessedAsync(
            Guid messageId,
            Guid lockToken,
            CancellationToken cancellationToken);

        Task<bool> MarkForRetryAsync(
            Guid messageId,
            Guid lockToken,
            DateTime nextAttemptAt,
            string error,
            CancellationToken cancellationToken);

        Task<bool> MarkFailedAsync(
            Guid messageId,
            Guid lockToken,
            string error,
            CancellationToken cancellationToken);

        Task<bool> MarkExpiredAsync(
            Guid messageId,
            Guid lockToken,
            CancellationToken cancellationToken);

        Task MarkExpiredMessagesAsync(
            DateTime now,
            CancellationToken cancellationToken);
    }
}