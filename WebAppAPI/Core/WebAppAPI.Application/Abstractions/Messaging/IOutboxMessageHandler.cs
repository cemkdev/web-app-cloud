namespace WebAppAPI.Application.Abstractions.Messaging
{
    public interface IOutboxMessageHandler
    {
        string MessageType { get; }

        Task HandleAsync(string payload, CancellationToken cancellationToken);
    }
}