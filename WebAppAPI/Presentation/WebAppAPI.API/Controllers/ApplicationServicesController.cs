using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppAPI.Application.Abstractions.Services.Configurations;
using WebAppAPI.Application.Consts;
using WebAppAPI.Application.CustomAttributes;
using WebAppAPI.Application.DTOs.AuthorizationDefinitions;
using WebAppAPI.Application.Enums;
using WebAppAPI.Domain.Constants;

namespace WebAppAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
    public class ApplicationServicesController(IApplicationService applicationService) : ControllerBase
    {
        [HttpGet("get-authorize-definition-endpoints")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.ApplicationServices, Definition = "Get Authorize Definition Endpoints", ActionType = ActionType.Read, AdminOnly = true)]
        public ActionResult<List<EndpointMenuDto>> GetAuthorizeDefinitionEndpoints()
        {
            List<EndpointMenuDto> endpointMenus = applicationService.GetAuthorizeDefinitionEndpoints(typeof(Program));

            return Ok(endpointMenus);
        }
    }
}
