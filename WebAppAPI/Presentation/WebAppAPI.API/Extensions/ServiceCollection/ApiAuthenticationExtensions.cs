using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using WebAppAPI.Domain.Constants;

namespace WebAppAPI.API.Extensions.ServiceCollection
{
    public static class ApiAuthenticationExtensions
    {
        public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidAudience = configuration["Token:Audience"],
                ValidIssuer = configuration["Token:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Token:SecurityKey"])),

                ClockSkew = TimeSpan.Zero,

                NameClaimType = ClaimTypes.Name
            };

            services.AddSingleton(tokenValidationParameters);

            // TODO-SECTION-3-AUTH: Review default authentication scheme.
            // Current setup keeps JwtBearerDefaults.AuthenticationScheme as default while JWT bearer is registered with AuthSchemes.Authenticated.
            // This is preserved intentionally in Section 2B to avoid behavior changes.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(AuthSchemes.Authenticated, options =>
                    {
                        options.TokenValidationParameters = tokenValidationParameters;

                        options.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                var accessToken = context.Request.Cookies["accessToken"];
                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    context.Token = accessToken;
                                }
                                return Task.CompletedTask;
                            }
                        };
                    });

            return services;
        }
    }
}
