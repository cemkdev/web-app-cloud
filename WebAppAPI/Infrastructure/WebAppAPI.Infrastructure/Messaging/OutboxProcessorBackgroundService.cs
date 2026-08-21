using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebAppAPI.Application.Abstractions.Messaging;
using WebAppAPI.Application.Abstractions.Messaging.Models;
using WebAppAPI.Application.Options.Outbox;

namespace WebAppAPI.Infrastructure.Messaging
{
    public sealed class OutboxProcessorBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxProcessorOptions> options,
        ILogger<OutboxProcessorBackgroundService> logger)
        : BackgroundService
    {
        private readonly OutboxProcessorOptions _options = options.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessBatchAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Unexpected error occurred while processing the outbox.");
                }

                await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
            }
        }

        private async Task ProcessBatchAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = scopeFactory.CreateScope();

            IOutboxStore store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();

            IOutboxMessageDispatcher dispatcher = scope.ServiceProvider.GetRequiredService<IOutboxMessageDispatcher>();

            DateTime now = DateTime.UtcNow;

            await store.MarkExpiredMessagesAsync(now, cancellationToken);

            IReadOnlyList<OutboxMessageEnvelope> messages = await store.ClaimPendingAsync(
                _options.BatchSize,
                TimeSpan.FromSeconds(_options.LockDurationSeconds),
                cancellationToken);

            foreach (OutboxMessageEnvelope message in messages)
                await ProcessMessageAsync(message, store, dispatcher, cancellationToken);
        }

        private async Task ProcessMessageAsync(
            OutboxMessageEnvelope message,
            IOutboxStore store,
            IOutboxMessageDispatcher dispatcher,
            CancellationToken cancellationToken)
        {
            if (message.ExpiresAt is not null && message.ExpiresAt <= DateTime.UtcNow)
            {
                await store.MarkExpiredAsync(message.Id, message.LockToken, cancellationToken);
                return;
            }

            try
            {
                await dispatcher.DispatchAsync(message, cancellationToken);

                bool markedProcessed = await store.MarkProcessedAsync(
                    message.Id,
                    message.LockToken,
                    cancellationToken);

                if (!markedProcessed)
                    logger.LogWarning("Outbox message {OutboxMessageId} was dispatched but its claim was no longer valid.", message.Id);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                await HandleFailureAsync(
                    message,
                    exception,
                    store,
                    cancellationToken);
            }
        }

        private async Task HandleFailureAsync(
            OutboxMessageEnvelope message,
            Exception exception,
            IOutboxStore store,
            CancellationToken cancellationToken)
        {
            int failedAttemptCount = message.AttemptCount + 1;
            string error = exception.ToString();

            if (failedAttemptCount >= _options.MaxAttempts)
            {
                await store.MarkFailedAsync(
                    message.Id,
                    message.LockToken,
                    error,
                    cancellationToken);

                logger.LogError(
                    exception,
                    "Outbox message {OutboxMessageId} failed permanently after {AttemptCount} attempts.",
                    message.Id,
                    failedAttemptCount);

                return;
            }

            TimeSpan retryDelay = CalculateRetryDelay(message.AttemptCount);
            DateTime nextAttemptAt = DateTime.UtcNow.Add(retryDelay);

            await store.MarkForRetryAsync(
                message.Id,
                message.LockToken,
                nextAttemptAt,
                error,
                cancellationToken);

            logger.LogWarning(
                exception,
                "Outbox message {OutboxMessageId} failed. Retry {AttemptCount}/{MaxAttempts} scheduled for {NextAttemptAt}.",
                message.Id,
                failedAttemptCount,
                _options.MaxAttempts,
                nextAttemptAt);
        }

        private TimeSpan CalculateRetryDelay(int previousAttemptCount)
        {
            double multiplier = Math.Pow(2, previousAttemptCount);

            return TimeSpan.FromSeconds(_options.InitialRetryDelaySeconds * multiplier);
        }
    }
}