using Microsoft.Extensions.Options;

namespace WebAppAPI.Application.Options.Storage
{
    public sealed class BaseStorageOptionsValidator : IValidateOptions<BaseStorageOptions>
    {
        public ValidateOptionsResult Validate(string? name, BaseStorageOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Url))
                return ValidateOptionsResult.Fail($"{BaseStorageOptions.SectionName}:Url is required.");

            if (!Uri.TryCreate(options.Url, UriKind.Absolute, out _))
                return ValidateOptionsResult.Fail($"{BaseStorageOptions.SectionName}:Url must be a valid absolute URL.");

            return ValidateOptionsResult.Success;
        }
    }
}
