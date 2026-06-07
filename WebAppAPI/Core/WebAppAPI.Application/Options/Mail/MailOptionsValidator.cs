using Microsoft.Extensions.Options;

namespace WebAppAPI.Application.Options.Mail
{
    public sealed class MailOptionsValidator : IValidateOptions<MailOptions>
    {
        public ValidateOptionsResult Validate(string? name, MailOptions options)
        {
            var hasAnyValue =
                !string.IsNullOrWhiteSpace(options.Username) ||
                !string.IsNullOrWhiteSpace(options.Password) ||
                options.Port.HasValue ||
                options.EnableSsl.HasValue ||
                !string.IsNullOrWhiteSpace(options.Host);

            // Mail configuration is optional, but if any value is provided, all required mail settings must be completed.
            if (!hasAnyValue)
                return ValidateOptionsResult.Success;

            if (string.IsNullOrWhiteSpace(options.Username))
                return ValidateOptionsResult.Fail($"{MailOptions.SectionName}:Username is required when mail configuration is provided.");

            if (string.IsNullOrWhiteSpace(options.Password))
                return ValidateOptionsResult.Fail($"{MailOptions.SectionName}:Password is required when mail configuration is provided.");

            if (!options.Port.HasValue)
                return ValidateOptionsResult.Fail($"{MailOptions.SectionName}:Port is required when mail configuration is provided.");

            if (options.Port <= 0)
                return ValidateOptionsResult.Fail($"{MailOptions.SectionName}:Port must be greater than zero.");

            if (!options.EnableSsl.HasValue)
                return ValidateOptionsResult.Fail($"{MailOptions.SectionName}:EnableSsl is required when mail configuration is provided.");

            if (string.IsNullOrWhiteSpace(options.Host))
                return ValidateOptionsResult.Fail($"{MailOptions.SectionName}:Host is required when mail configuration is provided.");

            return ValidateOptionsResult.Success;
        }
    }
}
