using Microsoft.Extensions.Options;

namespace WebAppAPI.Application.Options.Authentication
{
    public sealed class TokenExpirationOptionsValidator : IValidateOptions<TokenExpirationOptions>
    {
        public ValidateOptionsResult Validate(string? name, TokenExpirationOptions options)
        {
            if (options.AccessToken <= 0)
                return ValidateOptionsResult.Fail($"{TokenExpirationOptions.SectionName}:AccessToken must be greater than zero.");

            if (options.RefreshToken <= 0)
                return ValidateOptionsResult.Fail($"{TokenExpirationOptions.SectionName}:RefreshToken must be greater than zero.");

            if (options.RefreshBeforeTime <= 0)
                return ValidateOptionsResult.Fail($"{TokenExpirationOptions.SectionName}:RefreshBeforeTime must be greater than zero.");

            if (options.RefreshToken <= options.AccessToken)
                return ValidateOptionsResult.Fail($"{TokenExpirationOptions.SectionName}:RefreshToken must be greater than AccessToken.");

            if (options.RefreshBeforeTime >= options.AccessToken)
                return ValidateOptionsResult.Fail($"{TokenExpirationOptions.SectionName}:RefreshBeforeTime must be less than AccessToken.");

            return ValidateOptionsResult.Success;
        }
    }
}
