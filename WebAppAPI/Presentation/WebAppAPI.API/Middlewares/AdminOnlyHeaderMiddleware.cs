using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;
using WebAppAPI.Application.CustomAttributes;

namespace WebAppAPI.API.Middlewares
{
    public class AdminOnlyHeaderMiddleware
    {
        private const string AdminOnlyHeaderName = "X-Admin-Only";
        private const string ExposeHeadersName = "Access-Control-Expose-Headers";

        private readonly RequestDelegate _next;

        public AdminOnlyHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();

            if (endpoint != null)
            {
                var descriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (descriptor != null)
                {
                    var authorizeAttr = descriptor.MethodInfo.GetCustomAttribute<AuthorizeDefinitionAttribute>();
                    if (authorizeAttr != null)
                    {
                        bool isAdminOnly = authorizeAttr.AdminOnly;

                        // This middleware does not make authorization decisions.
                        // It only exposes the endpoint's AdminOnly metadata to the Angular client for 403 UX handling.
                        context.Response.OnStarting(() =>
                        {
                            context.Response.Headers[AdminOnlyHeaderName] = isAdminOnly.ToString().ToLowerInvariant();
                            EnsureHeaderIsExposed(context.Response, AdminOnlyHeaderName);

                            return Task.CompletedTask;
                        });

                        context.Items["IsAdminOnly"] = isAdminOnly;
                    }
                }
            }

            await _next(context);
        }

        private static void EnsureHeaderIsExposed(HttpResponse response, string headerName)
        {
            var exposedHeaders = response.Headers[ExposeHeadersName]
                .ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            if (!exposedHeaders.Contains(headerName, StringComparer.OrdinalIgnoreCase))
                exposedHeaders.Add(headerName);

            response.Headers[ExposeHeadersName] = string.Join(", ", exposedHeaders);
        }
    }
}
