using Microsoft.Extensions.Options;

namespace WebAppAPI.Application.Options.Storage
{
    public sealed class StorageOptionsValidator : IValidateOptions<StorageOptions>
    {
        public ValidateOptionsResult Validate(string? name, StorageOptions options)
        {
            if (!Enum.IsDefined(options.Provider))
                return ValidateOptionsResult.Fail($"{StorageOptions.SectionName}:Provider has an invalid value.");

            if (options.Provider == StorageProvider.Azure && string.IsNullOrWhiteSpace(options.Azure.ConnectionString))
                return ValidateOptionsResult.Fail($"{StorageOptions.SectionName}:Azure:ConnectionString is required when Azure storage provider is selected.");

            return ValidateOptionsResult.Success;
        }
    }
}
