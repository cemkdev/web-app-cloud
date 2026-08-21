using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Abstractions.Messaging;
using WebAppAPI.Application.Abstractions.Messaging.Models;
using WebAppAPI.Persistence.Contexts;

namespace WebAppAPI.Persistence.Outbox
{
    public sealed class OutboxStore(WebAppAPIDbContext context) : IOutboxStore
    {
        public async Task<IReadOnlyList<OutboxMessageEnvelope>> ClaimPendingAsync(int batchSize, TimeSpan lockDuration, CancellationToken cancellationToken)
        {
            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize));

            if (lockDuration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(lockDuration));

            DateTime now = DateTime.UtcNow;
            DateTime lockedUntil = now.Add(lockDuration);

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            List<OutboxMessage> messages = await context.OutboxMessages
                .FromSqlInterpolated($"""
                    SELECT *
                    FROM "OutboxMessages"
                    WHERE
                        (
                            ("Status" = {(int)OutboxMessageStatus.Pending} AND "NextAttemptAt" <= {now})
                            OR
                            ("Status" = {(int)OutboxMessageStatus.Processing} AND "LockedUntil" <= {now})
                        )
                        AND ("ExpiresAt" IS NULL OR "ExpiresAt" > {now})
                    ORDER BY "CreatedAt"
                    FOR UPDATE SKIP LOCKED
                    LIMIT {batchSize}
                    """)
                .ToListAsync(cancellationToken);

            foreach (OutboxMessage message in messages)
            {
                message.Status = OutboxMessageStatus.Processing;
                message.LockToken = Guid.NewGuid();
                message.LockedUntil = lockedUntil;
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return messages
                .Select(message => new OutboxMessageEnvelope
                {
                    Id = message.Id,
                    LockToken = message.LockToken!.Value,
                    Type = message.Type,
                    Payload = message.Payload,
                    AttemptCount = message.AttemptCount,
                    ExpiresAt = message.ExpiresAt
                })
                .ToList();
        }

        public Task<bool> MarkProcessedAsync(Guid messageId, Guid lockToken, CancellationToken cancellationToken)
            => UpdateClaimedMessageAsync(
                messageId,
                lockToken,
                message =>
                {
                    message.Status = OutboxMessageStatus.Processed;
                    message.ProcessedAt = DateTime.UtcNow;
                    message.LockToken = null;
                    message.LockedUntil = null;
                    message.LastError = null;
                },
                cancellationToken);

        public Task<bool> MarkForRetryAsync(Guid messageId, Guid lockToken, DateTime nextAttemptAt, string error, CancellationToken cancellationToken)
            => UpdateClaimedMessageAsync(
                messageId,
                lockToken,
                message =>
                {
                    message.Status = OutboxMessageStatus.Pending;
                    message.AttemptCount++;
                    message.NextAttemptAt = nextAttemptAt;
                    message.LockToken = null;
                    message.LockedUntil = null;
                    message.LastError = error;
                },
                cancellationToken);

        public Task<bool> MarkFailedAsync(Guid messageId, Guid lockToken, string error, CancellationToken cancellationToken)
            => UpdateClaimedMessageAsync(
                messageId,
                lockToken,
                message =>
                {
                    message.Status = OutboxMessageStatus.Failed;
                    message.AttemptCount++;
                    message.LockToken = null;
                    message.LockedUntil = null;
                    message.LastError = error;
                },
                cancellationToken);

        public Task<bool> MarkExpiredAsync(Guid messageId, Guid lockToken, CancellationToken cancellationToken)
            => UpdateClaimedMessageAsync(
                messageId,
                lockToken,
                message =>
                {
                    message.Status = OutboxMessageStatus.Expired;
                    message.LockToken = null;
                    message.LockedUntil = null;
                },
                cancellationToken);

        public async Task MarkExpiredMessagesAsync(DateTime now, CancellationToken cancellationToken)
        {
            await context.OutboxMessages
                .Where(message =>
                    message.ExpiresAt != null &&
                    message.ExpiresAt <= now &&
                    (message.Status == OutboxMessageStatus.Pending || message.Status == OutboxMessageStatus.Processing))
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            message => message.Status,
                            OutboxMessageStatus.Expired)
                        .SetProperty(
                            message => message.LockToken,
                            (Guid?)null)
                        .SetProperty(
                            message => message.LockedUntil,
                            (DateTime?)null),
                    cancellationToken);
        }

        #region Helpers
        private async Task<bool> UpdateClaimedMessageAsync(Guid messageId, Guid lockToken, Action<OutboxMessage> update, CancellationToken cancellationToken)
        {
            OutboxMessage? message = await context.OutboxMessages
                .SingleOrDefaultAsync(
                    message =>
                        message.Id == messageId &&
                        message.Status == OutboxMessageStatus.Processing &&
                        message.LockToken == lockToken,
                    cancellationToken);

            if (message is null)
                return false;

            update(message);

            await context.SaveChangesAsync(cancellationToken);

            return true;
        }
        #endregion
    }
}