using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Abstractions.Services.Authentications;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Domain.Entities.Identity;
using WebAppAPI.Persistence.Contexts;
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

            // Domain entity repositories
            services.AddScoped<IOrderReadRepository, OrderReadRepository>();
            services.AddScoped<IProductReadRepository, ProductReadRepository>();
            services.AddScoped<IBasketReadRepository, BasketReadRepository>();
            services.AddScoped<IBasketItemReadRepository, BasketItemReadRepository>();
            services.AddScoped<IOrderStatusHistoryReadRepository, OrderStatusHistoryReadRepository>();
            services.AddScoped<IEndpointReadRepository, EndpointReadRepository>();
            services.AddScoped<IMenuReadRepository, MenuReadRepository>();
            services.AddScoped<IProductImageFileReadRepository, ProductImageFileReadRepository>();

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
        }
    }
}
