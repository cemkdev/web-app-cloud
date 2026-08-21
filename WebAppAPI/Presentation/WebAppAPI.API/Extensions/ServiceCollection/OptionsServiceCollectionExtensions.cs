using Microsoft.Extensions.Options;
using WebAppAPI.API.Options.Hosting;
using WebAppAPI.Application.Options.Authentication;
using WebAppAPI.Application.Options.Client;
using WebAppAPI.Application.Options.IdentityTokens;
using WebAppAPI.Application.Options.Mail;
using WebAppAPI.Application.Options.Outbox;
using WebAppAPI.Application.Options.Storage;

namespace WebAppAPI.API.Extensions.ServiceCollection
{
    public static class OptionsServiceCollectionExtensions
    {
        public static IServiceCollection AddApiConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<ClientOptions>()
                .Bind(configuration.GetSection(ClientOptions.SectionName))
                .ValidateOnStart();

            services.AddOptions<HostingOptions>()
                .Bind(configuration.GetSection(HostingOptions.SectionName))
                .ValidateOnStart();

            services.AddOptions<TokenOptions>()
                .Bind(configuration.GetSection(TokenOptions.SectionName))
                .ValidateOnStart();

            services.AddOptions<TokenExpirationOptions>()
                .Bind(configuration.GetSection(TokenExpirationOptions.SectionName))
                .ValidateOnStart();

            services.AddOptions<IdentityTokenOptions>()
                .Bind(configuration.GetSection(IdentityTokenOptions.SectionName))
                .ValidateOnStart();

            services.AddOptions<AuthCookieOptions>()
                .Bind(configuration.GetSection(AuthCookieOptions.SectionName))
                .ValidateOnStart();

            services.AddOptions<ExternalLoginOptions>()
                .Bind(configuration.GetSection(ExternalLoginOptions.SectionName))
                .ValidateOnStart();

            services.AddOptions<StorageOptions>()
                .Bind(configuration.GetSection(StorageOptions.SectionName))
                .ValidateOnStart();

            services.AddOptions<BaseStorageOptions>()
                .Bind(configuration.GetSection(BaseStorageOptions.SectionName))
                .ValidateOnStart();

            services.AddOptions<MailOptions>()
                .Bind(configuration.GetSection(MailOptions.SectionName))
                .ValidateOnStart();

            services.AddOptions<EmailDisplayNameOptions>()
                .Bind(configuration.GetSection(EmailDisplayNameOptions.SectionName))
                .ValidateOnStart();

            services.AddOptions<OutboxProcessorOptions>()
                .Bind(configuration.GetSection(OutboxProcessorOptions.SectionName))
                .ValidateOnStart();

            services.AddSingleton<IValidateOptions<ClientOptions>, ClientOptionsValidator>();
            services.AddSingleton<IValidateOptions<TokenOptions>, TokenOptionsValidator>();
            services.AddSingleton<IValidateOptions<TokenExpirationOptions>, TokenExpirationOptionsValidator>();
            services.AddSingleton<IValidateOptions<IdentityTokenOptions>, IdentityTokenOptionsValidator>();
            services.AddSingleton<IValidateOptions<StorageOptions>, StorageOptionsValidator>();
            services.AddSingleton<IValidateOptions<BaseStorageOptions>, BaseStorageOptionsValidator>();
            services.AddSingleton<IValidateOptions<MailOptions>, MailOptionsValidator>();
            services.AddSingleton<IValidateOptions<ExternalLoginOptions>, ExternalLoginOptionsValidator>();
            services.AddSingleton<IValidateOptions<OutboxProcessorOptions>, OutboxProcessorOptionsValidator>();

            return services;
        }
    }
}
