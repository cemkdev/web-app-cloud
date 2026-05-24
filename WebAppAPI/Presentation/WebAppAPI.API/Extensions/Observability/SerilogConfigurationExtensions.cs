using Serilog;
using Serilog.Events;
using WebAppAPI.API.Options.Observability;

namespace WebAppAPI.API.Extensions.Observability
{
    public static class SerilogConfigurationExtensions
    {
        public static IHostBuilder UseConfiguredSerilog(
            this IHostBuilder hostBuilder,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            if (hostBuilder is null)
                throw new ArgumentNullException(nameof(hostBuilder));

            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            if (environment is null)
                throw new ArgumentNullException(nameof(environment));

            return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
            {
                var observabilityOptions = configuration
                                                .GetSection(ObservabilityOptions.SectionName)
                                                .Get<ObservabilityOptions>()
                                                ?? new ObservabilityOptions();

                ConfigureMinimumLevels(loggerConfiguration);

                loggerConfiguration
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("ApplicationName", observabilityOptions.ApplicationName)
                    .Enrich.WithProperty("Environment", environment.EnvironmentName)
                    .MinimumLevel.Information()
                    .WriteTo.Console();
            });
        }

        private static void ConfigureMinimumLevels(LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning);
        }
    }
}
