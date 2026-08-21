using Microsoft.Extensions.Options;
using System.Text.Json;
using WebAppAPI.Application.Abstractions.Messaging;
using WebAppAPI.Application.Options.Outbox;
using WebAppAPI.Persistence.Contexts;

namespace WebAppAPI.Persistence.Outbox
{
    public sealed class OutboxWriter(WebAppAPIDbContext context, IOptions<OutboxProcessorOptions> outboxOptions) : IOutboxWriter
    {
        private readonly OutboxProcessorOptions _outboxOptions = outboxOptions.Value;

        public async Task EnqueueAsync<TPayload>(
            string type,
            TPayload payload,
            string deduplicationKey,
            DateTime? expiresAt,
            CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(type);
            ArgumentException.ThrowIfNullOrWhiteSpace(deduplicationKey);
            ArgumentNullException.ThrowIfNull(payload);

            DateTime now = DateTime.UtcNow;

            OutboxMessage message = new()
            {
                Id = Guid.NewGuid(),

                Type = type,
                Payload = JsonSerializer.Serialize(payload),
                DeduplicationKey = deduplicationKey,

                Status = OutboxMessageStatus.Pending,
                AttemptCount = 0,

                CreatedAt = now,
                NextAttemptAt = now,
                ExpiresAt = expiresAt ?? now.AddMinutes(_outboxOptions.DefaultMessageLifetimeMinutes)
            };

            await context.OutboxMessages.AddAsync(message, cancellationToken);
        }
    }
}