using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.CustomAttributes;
using WebAppAPI.Application.Repositories;

namespace WebAppAPI.API.Filters
{
    public class RolePermissionFilter : IAsyncActionFilter
    {
        readonly IUserService _userService;
        readonly IEndpointReadRepository _endpointReadRepository;

        public RolePermissionFilter(IUserService userService, IEndpointReadRepository endpointReadRepository)
        {
            _userService = userService;
            _endpointReadRepository = endpointReadRepository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var username = context.HttpContext.User.Identity?.Name;

            if (!string.IsNullOrEmpty(username))
            {
                var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
                var authorizeDefinitionAttribute = descriptor?.MethodInfo.GetCustomAttribute(typeof(AuthorizeDefinitionAttribute)) as AuthorizeDefinitionAttribute;
                var httpAttribute = descriptor?.MethodInfo.GetCustomAttribute(typeof(HttpMethodAttribute)) as HttpMethodAttribute;

                if (authorizeDefinitionAttribute == null)
                {
                    await next();
                    return;
                }

                var code = $"{(httpAttribute != null ? httpAttribute.HttpMethods.First() : HttpMethods.Get)}.{authorizeDefinitionAttribute.ActionType.ToString()}.{authorizeDefinitionAttribute.Definition.Replace(" ", "")}";

                var endpoint = await _endpointReadRepository.Table
                                        .Include(e => e.Menu)
                                        .FirstOrDefaultAsync(e => e.Code == code);

                if (endpoint?.AdminOnly == true)
                {
                    var hasAdminAccess = await _userService.HasAdminAccessAsync(username);
                    if (!hasAdminAccess)
                    {
                        context.Result = new ObjectResult(new { message = "Only administrators can access this endpoint." })
                        {
                            StatusCode = StatusCodes.Status403Forbidden
                        };
                        return;
                    }
                }

                var hasRole = await _userService.HasRolePermissionAsync(username, code);
                if (!hasRole)
                    context.Result = new ObjectResult(new { message = "You are not authorized to view this page." })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                else
                    await next();
            }
            else
                await next();
        }
    }
}
