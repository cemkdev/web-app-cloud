using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebAppAPI.API.Filters;
using WebAppAPI.Application.Validators.Products;
using WebAppAPI.Infrastructure.Filters;

namespace WebAppAPI.API.Extensions.ServiceCollection
{
    public static class ApiServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigin = configuration["AngularClientUrl"]
                                ?? throw new InvalidOperationException("AngularClientUrl configuration is missing.");

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(allowedOrigin)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            services.AddControllers(options =>
            {
                options.Filters.Add<ValidationFilter>();
                options.Filters.Add<RolePermissionFilter>();
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddValidatorsFromAssemblyContaining<ProductCreateValidator>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            return services;
        }
    }
}
