using Microsoft.Extensions.Options;

namespace WebAppAPI.Application.Options.Authentication
{
    public sealed class ExternalLoginOptionsValidator : IValidateOptions<ExternalLoginOptions>
    {
        public ValidateOptionsResult Validate(string? name, ExternalLoginOptions options)
        {
            var facebookHasAnyValue =
                !string.IsNullOrWhiteSpace(options.Facebook.ClientId) ||
                !string.IsNullOrWhiteSpace(options.Facebook.ClientSecret);

            // Facebook login configuration is optional, but partial configuration is invalid.
            if (facebookHasAnyValue)
            {
                if (string.IsNullOrWhiteSpace(options.Facebook.ClientId))
                    return ValidateOptionsResult.Fail($"{ExternalLoginOptions.SectionName}:Facebook:ClientId is required when Facebook login configuration is provided.");

                if (string.IsNullOrWhiteSpace(options.Facebook.ClientSecret))
                    return ValidateOptionsResult.Fail($"{ExternalLoginOptions.SectionName}:Facebook:ClientSecret is required when Facebook login configuration is provided.");
            }

            // Google currently has only ClientId, so there is no partial configuration rule to validate.
            // It is also optional.
            return ValidateOptionsResult.Success;
        }
    }
}
