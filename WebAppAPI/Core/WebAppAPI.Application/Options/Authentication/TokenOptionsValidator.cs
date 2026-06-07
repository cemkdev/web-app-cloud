using Microsoft.Extensions.Options;

namespace WebAppAPI.Application.Options.Authentication
{
    public sealed class TokenOptionsValidator : IValidateOptions<TokenOptions>
    {
        public ValidateOptionsResult Validate(string? name, TokenOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Audience))
                return ValidateOptionsResult.Fail($"{TokenOptions.SectionName}:Audience is required.");

            if (string.IsNullOrWhiteSpace(options.Issuer))
                return ValidateOptionsResult.Fail($"{TokenOptions.SectionName}:Issuer is required.");

            if (string.IsNullOrWhiteSpace(options.SecurityKey))
                return ValidateOptionsResult.Fail($"{TokenOptions.SectionName}:SecurityKey is required.");

            if (options.SecurityKey.Length < 32)
                return ValidateOptionsResult.Fail($"{TokenOptions.SectionName}:SecurityKey must be at least 32 characters.");

            return ValidateOptionsResult.Success;
        }
    }
}
