using Microsoft.Extensions.Options;

namespace WebAppAPI.API.Options.Observability.Validation
{
    public sealed class ObservabilityOptionsValidator : IValidateOptions<ObservabilityOptions>
    {
        public ValidateOptionsResult Validate(string? name, ObservabilityOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ApplicationName))
                return ValidateOptionsResult.Fail("Observability:ApplicationName is required.");

            if (options.Logging is null)
                return ValidateOptionsResult.Fail("Observability:Logging section is required.");

            if (string.IsNullOrWhiteSpace(options.Logging.Provider))
                return ValidateOptionsResult.Fail("Observability:Logging:Provider is required.");

            return options.Logging.Provider switch
            {
                LoggingProviders.Seq => ValidateSeq(options),
                LoggingProviders.Elastic => ValidateElastic(options),
                LoggingProviders.ConsoleOnly => ValidateOptionsResult.Success,
                _ => ValidateOptionsResult.Fail(
                    $"Unsupported observability logging provider: '{options.Logging.Provider}'. " +
                    $"Supported providers: {LoggingProviders.Seq}, {LoggingProviders.Elastic}, {LoggingProviders.ConsoleOnly}.")
            };
        }

        private static ValidateOptionsResult ValidateSeq(ObservabilityOptions options)
        {
            if (options.Logging.Seq is null)
                return ValidateOptionsResult.Fail("Observability:Logging:Seq section is required when provider is Seq.");

            if (string.IsNullOrWhiteSpace(options.Logging.Seq.ServerUrl))
                return ValidateOptionsResult.Fail("Observability:Logging:Seq:ServerUrl is required when provider is Seq.");

            if (!Uri.TryCreate(options.Logging.Seq.ServerUrl, UriKind.Absolute, out _))
                return ValidateOptionsResult.Fail("Observability:Logging:Seq:ServerUrl must be a valid absolute URI.");

            return ValidateOptionsResult.Success;
        }

        private static ValidateOptionsResult ValidateElastic(ObservabilityOptions options)
        {
            if (options.Logging.Elastic is null)
                return ValidateOptionsResult.Fail("Observability:Logging:Elastic section is required when provider is Elastic.");

            if (string.IsNullOrWhiteSpace(options.Logging.Elastic.Uri))
                return ValidateOptionsResult.Fail("Observability:Logging:Elastic:Uri is required when provider is Elastic.");

            if (!Uri.TryCreate(options.Logging.Elastic.Uri, UriKind.Absolute, out _))
                return ValidateOptionsResult.Fail("Observability:Logging:Elastic:Uri must be a valid absolute URI.");

            return ValidateOptionsResult.Success;
        }
    }
}
