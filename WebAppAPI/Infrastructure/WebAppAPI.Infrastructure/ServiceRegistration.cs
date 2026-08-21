using Microsoft.Extensions.DependencyInjection;
using WebAppAPI.Application.Abstractions.Messaging;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Abstractions.Services.Configurations;
using WebAppAPI.Application.Abstractions.Token;
using WebAppAPI.Infrastructure.Messaging;
using WebAppAPI.Infrastructure.Messaging.Handlers;
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
            serviceCollection.AddSingleton<StorageFileNameGenerator>();
            serviceCollection.AddScoped<ITokenHandler, TokenHandler>();
            serviceCollection.AddScoped<IMailService, MailService>();
            serviceCollection.AddScoped<IApplicationService, ApplicationService>();
            serviceCollection.AddScoped<IQRCodeService, QRCodeService>();
            serviceCollection.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher>();
            serviceCollection.AddHostedService<OutboxProcessorBackgroundService>();
            serviceCollection.AddScoped<IOutboxMessageHandler, OrderStatusUpdateMailMessageHandler>();
            serviceCollection.AddScoped<IOutboxMessageHandler, PasswordResetMailMessageHandler>();
        }
    }
}
