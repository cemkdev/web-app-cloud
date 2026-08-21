using Microsoft.Extensions.Options;

namespace WebAppAPI.Application.Options.IdentityTokens
{
    public sealed class IdentityTokenOptionsValidator : IValidateOptions<IdentityTokenOptions>
    {
        public ValidateOptionsResult Validate(string? name, IdentityTokenOptions options)
        {
            if (options.LifetimeMinutes <= 0)
                return ValidateOptionsResult.Fail($"{IdentityTokenOptions.SectionName}:LifetimeMinutes must be greater than zero.");

            return ValidateOptionsResult.Success;
        }
    }
}
