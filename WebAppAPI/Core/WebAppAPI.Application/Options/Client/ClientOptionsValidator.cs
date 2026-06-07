using Microsoft.Extensions.Options;

namespace WebAppAPI.Application.Options.Client
{
    public sealed class ClientOptionsValidator : IValidateOptions<ClientOptions>
    {
        public ValidateOptionsResult Validate(string? name, ClientOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.AngularUrl))
                return ValidateOptionsResult.Fail($"{ClientOptions.SectionName}:AngularUrl is required.");

            if (!Uri.TryCreate(options.AngularUrl, UriKind.Absolute, out _))
                return ValidateOptionsResult.Fail($"{ClientOptions.SectionName}:AngularUrl must be a valid absolute URL.");

            return ValidateOptionsResult.Success;
        }
    }
}
