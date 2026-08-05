using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;
using WebAppAPI.Application.Abstractions.Services.Configurations;
using WebAppAPI.Application.CustomAttributes;
using WebAppAPI.Application.DTOs.AuthorizationDefinitions;

namespace WebAppAPI.Infrastructure.Services.Configurations
{
    public sealed class ApplicationService : IApplicationService
    {
        public List<EndpointMenuDto> GetAuthorizeDefinitionEndpoints(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            Assembly assembly = type.Assembly;

            IEnumerable<Type> controllers = assembly
                .GetTypes()
                .Where(controllerType =>
                    controllerType.IsAssignableTo(typeof(ControllerBase)) &&
                    !controllerType.IsAbstract);

            List<EndpointMenuDto> endpointMenus = [];

            foreach (Type controller in controllers)
            {
                IEnumerable<MethodInfo> actions = controller
                    .GetMethods()
                    .Where(method =>
                        method.IsDefined(
                            typeof(AuthorizeDefinitionAttribute),
                            inherit: true));

                foreach (MethodInfo action in actions)
                {
                    AuthorizeDefinitionAttribute authorizeDefinitionAttribute =
                        action.GetCustomAttribute<AuthorizeDefinitionAttribute>(inherit: true)
                        ?? throw new InvalidOperationException($"AuthorizeDefinitionAttribute could not be read from '{action.DeclaringType?.Name}.{action.Name}'.");

                    if (string.IsNullOrWhiteSpace(authorizeDefinitionAttribute.Menu))
                        throw new InvalidOperationException($"Menu definition is missing on '{action.DeclaringType?.Name}.{action.Name}'.");

                    if (string.IsNullOrWhiteSpace(authorizeDefinitionAttribute.Definition))
                        throw new InvalidOperationException($"Endpoint definition is missing on '{action.DeclaringType?.Name}.{action.Name}'.");

                    EndpointMenuDto? endpointMenu = endpointMenus.FirstOrDefault(endpointMenu => endpointMenu.Name == authorizeDefinitionAttribute.Menu);

                    if (endpointMenu is null)
                    {
                        endpointMenu = new EndpointMenuDto
                        {
                            Name = authorizeDefinitionAttribute.Menu
                        };
                        endpointMenus.Add(endpointMenu);
                    }

                    HttpMethodAttribute? httpMethodAttribute = action
                        .GetCustomAttributes(inherit: true)
                        .OfType<HttpMethodAttribute>()
                        .FirstOrDefault();

                    string httpType = httpMethodAttribute?.HttpMethods.FirstOrDefault() ?? HttpMethods.Get;

                    string code = $"{httpType}.{authorizeDefinitionAttribute.ActionType}.{authorizeDefinitionAttribute.Definition.Replace(" ", "")}";

                    EndpointDefinitionDto endpointDefinition = new()
                    {
                        ActionType = authorizeDefinitionAttribute.ActionType,
                        HttpType = httpType,
                        Definition = authorizeDefinitionAttribute.Definition,
                        Code = code,
                        AdminOnly = authorizeDefinitionAttribute.AdminOnly
                    };

                    endpointMenu.Endpoints.Add(endpointDefinition);
                }
            }

            return endpointMenus;
        }
    }
}
