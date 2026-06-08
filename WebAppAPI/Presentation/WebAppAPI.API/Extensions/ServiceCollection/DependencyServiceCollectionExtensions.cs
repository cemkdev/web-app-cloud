using WebAppAPI.Application;
using WebAppAPI.Application.Options.Storage;
using WebAppAPI.Infrastructure;
using WebAppAPI.Infrastructure.Services.Storage.Azure;
using WebAppAPI.Infrastructure.Services.Storage.Local;
using WebAppAPI.Persistence;
using WebAppAPI.SignalR;

namespace WebAppAPI.API.Extensions.ServiceCollection
{
    public static class DependencyServiceCollectionExtensions
    {
        public static IServiceCollection AddWebAppApiDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services.AddPersistenceServices(configuration);
            services.AddInfrastructureServices();
            services.AddApplicationServices();
            services.AddSignalRServices();

            services.AddConfiguredStorage(configuration);

            return services;
        }

        #region Helper
        private static void AddConfiguredStorage(this IServiceCollection services, IConfiguration configuration)
        {
            var storageOptions = configuration
                                    .GetSection(StorageOptions.SectionName)
                                    .Get<StorageOptions>()
                                    ?? throw new InvalidOperationException($"{StorageOptions.SectionName} configuration is missing.");

            switch (storageOptions.Provider)
            {
                case StorageProvider.Local:
                    services.AddStorage<LocalStorage>();
                    break;

                case StorageProvider.Azure:
                    services.AddStorage<AzureStorage>();
                    break;

                default:
                    throw new InvalidOperationException($"{StorageOptions.SectionName}:Provider has an invalid value.");
            }
        }
        #endregion
    }
}
