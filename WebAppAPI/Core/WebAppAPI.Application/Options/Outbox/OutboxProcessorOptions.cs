namespace WebAppAPI.Application.Options.Outbox
{
    public sealed class OutboxProcessorOptions
    {
        public const string SectionName = "OutboxProcessor";

        public int BatchSize { get; set; } = 20;
        public int PollingIntervalSeconds { get; set; } = 5;
        public int LockDurationSeconds { get; set; } = 60;

        public int MaxAttempts { get; set; } = 5;
        public int InitialRetryDelaySeconds { get; set; } = 10;

        public int DefaultMessageLifetimeMinutes { get; set; } = 1440;
    }
}