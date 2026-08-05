using WebAppAPI.Application.DTOs.AuthorizationDefinitions;

namespace WebAppAPI.Application.Abstractions.Services.Configurations
{
    public interface IApplicationService
    {
        /// <summary>
        /// It scans all endpoints and sends them to the client before initially adding them to the database.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        List<EndpointMenuDto> GetAuthorizeDefinitionEndpoints(Type type);
    }
}
