using Microsoft.Extensions.DependencyInjection;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Abstractions.Services.Configurations;
using WebAppAPI.Application.Abstractions.Storage;
using WebAppAPI.Application.Abstractions.Token;
using WebAppAPI.Infrastructure.Services;
using WebAppAPI.Infrastructure.Services.Configurations;
using WebAppAPI.Infrastructure.Services.Storage;
using WebAppAPI.Infrastructure.Services.Token;

namespace WebAppAPI.Infrastructure
{
    public static class ServiceRegistration
    {
        public static void AddInfrastructureServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IStorageService, StorageService>();
            serviceCollection.AddScoped<ITokenHandler, TokenHandler>();
            serviceCollection.AddScoped<IMailService, MailService>();
            serviceCollection.AddScoped<IApplicationService, ApplicationService>();
            serviceCollection.AddScoped<IQRCodeService, QRCodeService>();
        }

        public static void AddStorage<T>(this IServiceCollection serviceCollection) where T : Storage, IStorage
        {
            serviceCollection.AddScoped<IStorage, T>();
        }
    }
}
