using WebAppAPI.Application.Abstractions.Messaging.Models;

namespace WebAppAPI.Application.Abstractions.Messaging
{
    public interface IOutboxMessageDispatcher
    {
        Task DispatchAsync(OutboxMessageEnvelope message, CancellationToken cancellationToken);
    }
}