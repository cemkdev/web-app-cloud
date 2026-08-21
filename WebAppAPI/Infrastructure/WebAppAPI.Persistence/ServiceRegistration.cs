using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebAppAPI.Application.Abstractions.Messaging;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Abstractions.Services.Authentications;
using WebAppAPI.Application.Options.IdentityTokens;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Domain.Entities.Identity;
using WebAppAPI.Persistence.Contexts;
using WebAppAPI.Persistence.Outbox;
using WebAppAPI.Persistence.Repositories;
using WebAppAPI.Persistence.Services;

namespace WebAppAPI.Persistence
{
    public static class ServiceRegistration
    {
        public static void AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<WebAppAPIDbContext>(options =>
                        options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IWriteRepository<>), typeof(WriteRepository<>));

            services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 3;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
            }).AddEntityFrameworkStores<WebAppAPIDbContext>()
            .AddDefaultTokenProviders(); // AddDefaultTokenProviders is required by UserManager.GeneratePasswordResetTokenAsync() in AuthService.

            services.AddOptions<DataProtectionTokenProviderOptions>()
                .Configure<IOptions<IdentityTokenOptions>>((options, identityTokenOptions) =>
                {
                    options.TokenLifespan = TimeSpan.FromMinutes(identityTokenOptions.Value.LifetimeMinutes);
                });

            // Domain entity repositories
            services.AddScoped<IEndpointReadRepository, EndpointReadRepository>();
            services.AddScoped<IMenuReadRepository, MenuReadRepository>();
            services.AddScoped<IProductReadRepository, ProductReadRepository>();
            services.AddScoped<IProductWriteRepository, ProductWriteRepository>();
            services.AddScoped<IProductImageFileReadRepository, ProductImageFileReadRepository>();
            services.AddScoped<IBasketItemReadRepository, BasketItemReadRepository>();
            services.AddScoped<IBasketReadRepository, BasketReadRepository>();
            services.AddScoped<IOrderReadRepository, OrderReadRepository>();
            services.AddScoped<IOrderWriteRepository, OrderWriteRepository>();
            services.AddScoped<IOrderItemSnapshotReadRepository, OrderItemSnapshotReadRepository>();
            services.AddScoped<IOrderItemSnapshotWriteRepository, OrderItemSnapshotWriteRepository>();
            services.AddScoped<IOrderStatusHistoryReadRepository, OrderStatusHistoryReadRepository>();

            // Authorization services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IInternalAuthentication, AuthService>();
            services.AddScoped<IExternalAuthentication, AuthService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPermissionService, PermissionService>();

            // Endpoint/Menu entities services
            services.AddScoped<IEndpointService, EndpointService>();

            // Basket/Order entities services
            services.AddScoped<IBasketService, BasketService>();
            services.AddScoped<IOrderService, OrderService>();

            // Product entity service
            services.AddScoped<IProductService, ProductService>();

            // Outbox
            services.AddScoped<IOutboxWriter, OutboxWriter>();
            services.AddScoped<IOutboxStore, OutboxStore>();
        }
    }
}
