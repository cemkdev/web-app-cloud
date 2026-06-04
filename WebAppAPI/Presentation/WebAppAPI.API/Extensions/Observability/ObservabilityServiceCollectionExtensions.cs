using Microsoft.Extensions.Options;
using WebAppAPI.API.Options.Observability;
using WebAppAPI.API.Options.Observability.Validation;

namespace WebAppAPI.API.Extensions.Observability
{
    public static class ObservabilityServiceCollectionExtensions
    {
        public static IServiceCollection AddApiObservability(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<ObservabilityOptions>()
                    .Bind(configuration.GetSection(ObservabilityOptions.SectionName))
                    .ValidateOnStart();

            services.AddSingleton<IValidateOptions<ObservabilityOptions>, ObservabilityOptionsValidator>();

            return services;
        }
    }
}
