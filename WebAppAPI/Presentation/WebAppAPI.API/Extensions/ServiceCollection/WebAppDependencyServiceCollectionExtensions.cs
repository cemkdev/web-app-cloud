using WebAppAPI.Application;
using WebAppAPI.Infrastructure;
using WebAppAPI.Infrastructure.Services.Storage.Local;
using WebAppAPI.Persistence;
using WebAppAPI.SignalR;

namespace WebAppAPI.API.Extensions.ServiceCollection
{
    public static class WebAppDependencyServiceCollectionExtensions
    {
        public static IServiceCollection AddWebAppApiDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();

            services.AddPersistenceServices(configuration);
            services.AddInfrastructureServices();
            services.AddApplicationServices();
            services.AddSignalRServices();

            services.AddStorage<LocalStorage>();

            return services;
        }
    }
}
