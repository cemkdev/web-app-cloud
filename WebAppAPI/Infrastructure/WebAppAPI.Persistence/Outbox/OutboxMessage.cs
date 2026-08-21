namespace WebAppAPI.Persistence.Outbox
{
    public sealed class OutboxMessage
    {
        public Guid Id { get; set; }

        public required string Type { get; set; }
        public required string Payload { get; set; }
        public required string DeduplicationKey { get; set; }

        public OutboxMessageStatus Status { get; set; }

        public int AttemptCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? NextAttemptAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public Guid? LockToken { get; set; }
        public DateTime? LockedUntil { get; set; }

        public string? LastError { get; set; }
    }
}
