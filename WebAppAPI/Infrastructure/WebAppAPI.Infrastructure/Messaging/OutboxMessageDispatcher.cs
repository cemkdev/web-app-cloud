using WebAppAPI.Application.Abstractions.Messaging;
using WebAppAPI.Application.Abstractions.Messaging.Models;

namespace WebAppAPI.Infrastructure.Messaging
{
    public sealed class OutboxMessageDispatcher(IEnumerable<IOutboxMessageHandler> handlers) : IOutboxMessageDispatcher
    {
        private readonly IReadOnlyDictionary<string, IOutboxMessageHandler> _handlers =
            handlers.ToDictionary(handler => handler.MessageType, StringComparer.Ordinal);

        public Task DispatchAsync(OutboxMessageEnvelope message, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(message);

            if (!_handlers.TryGetValue(message.Type, out IOutboxMessageHandler? handler))
                throw new InvalidOperationException($"No outbox message handler is registered for message type '{message.Type}'.");

            return handler.HandleAsync(message.Payload, cancellationToken);
        }
    }
}