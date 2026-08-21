using Microsoft.Extensions.Options;

namespace WebAppAPI.Application.Options.Outbox
{
    public sealed class OutboxProcessorOptionsValidator : IValidateOptions<OutboxProcessorOptions>
    {
        public ValidateOptionsResult Validate(string? name, OutboxProcessorOptions options)
        {
            if (options.BatchSize <= 0)
                return ValidateOptionsResult.Fail($"{OutboxProcessorOptions.SectionName}:BatchSize must be greater than zero.");

            if (options.PollingIntervalSeconds <= 0)
                return ValidateOptionsResult.Fail($"{OutboxProcessorOptions.SectionName}:PollingIntervalSeconds must be greater than zero.");

            if (options.LockDurationSeconds <= 0)
                return ValidateOptionsResult.Fail($"{OutboxProcessorOptions.SectionName}:LockDurationSeconds must be greater than zero.");

            if (options.MaxAttempts <= 0)
                return ValidateOptionsResult.Fail($"{OutboxProcessorOptions.SectionName}:MaxAttempts must be greater than zero.");

            if (options.InitialRetryDelaySeconds <= 0)
                return ValidateOptionsResult.Fail($"{OutboxProcessorOptions.SectionName}:InitialRetryDelaySeconds must be greater than zero.");

            if (options.DefaultMessageLifetimeMinutes <= 0)
                return ValidateOptionsResult.Fail($"{OutboxProcessorOptions.SectionName}:DefaultMessageLifetimeMinutes must be greater than zero.");

            return ValidateOptionsResult.Success;
        }
    }
}