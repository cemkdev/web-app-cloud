namespace WebAppAPI.Application.Abstractions.Messaging.Models
{
    public sealed class OutboxMessageEnvelope
    {
        public required Guid Id { get; init; }
        public required Guid LockToken { get; init; }
        public required string Type { get; init; }
        public required string Payload { get; init; }

        public int AttemptCount { get; init; }
        public DateTime? ExpiresAt { get; init; }
    }
}
