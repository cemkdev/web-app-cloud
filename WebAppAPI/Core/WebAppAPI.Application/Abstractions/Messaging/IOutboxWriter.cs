namespace WebAppAPI.Application.Abstractions.Messaging
{
    public interface IOutboxWriter
    {
        Task EnqueueAsync<TPayload>(
            string type,
            TPayload payload,
            string deduplicationKey,
            DateTime? expiresAt,
            CancellationToken cancellationToken);
    }
}
