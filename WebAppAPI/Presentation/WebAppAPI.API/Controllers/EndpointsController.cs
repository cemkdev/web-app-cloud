using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Consts;
using WebAppAPI.Application.CustomAttributes;
using WebAppAPI.Application.Enums;
using WebAppAPI.Application.Features.Endpoints.Commands.AssignRoleEndpoint;
using WebAppAPI.Application.Features.Endpoints.Queries.GetRolesEndpoints;
using WebAppAPI.Domain.Constants;
using WebAppAPI.Domain.Entities.Identity;

namespace WebAppAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
    public class EndpointsController : ControllerBase
    {
        readonly IMediator _mediator;
        readonly IEndpointService _endpointService;
        readonly UserManager<AppUser> _userManager;
        readonly RoleManager<AppRole> _roleManager;

        public EndpointsController(IMediator mediator, IEndpointService endpointService, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _mediator = mediator;
            _endpointService = endpointService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("get-roles-endpoints")]
        //[AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Endpoints, Definition = "Get Roles and Endpoints", ActionType = ActionType.Read, AdminOnly = true)]
        public async Task<IActionResult> GetRolesEndpoints([FromQuery] GetRolesEndpointsQueryRequest getRolesEndpointsQueryRequest)
        {
            GetRolesEndpointsQueryResponse response = await _mediator.Send(getRolesEndpointsQueryRequest);
            return Ok(response);
        }

        [HttpPost("assign-role-endpoints")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Endpoints, Definition = "Assign Roles to Endpoints", ActionType = ActionType.Write, AdminOnly = true)]
        public async Task<IActionResult> AssignRoleEndpoints([FromBody] AssignRoleEndpointCommandRequest assignRoleEndpointCommandRequest)
        {
            assignRoleEndpointCommandRequest.Type = typeof(Program);
            AssignRoleEndpointCommandResponse response = await _mediator.Send(assignRoleEndpointCommandRequest);
            return Ok(response);
        }

        [HttpGet("has-access")]
        public async Task<IActionResult> HasAccess([FromQuery] string menuName)
        {
            var username = User.Identity.Name;
            var result = await _endpointService.HasAccessToMenuAsync(username, menuName);
            return Ok(new { hasAccess = result });
        }

        [HttpGet("accessible-menus")]
        public async Task<IActionResult> GetAccessibleAdminSidebarMenus()
        {
            var username = User.Identity.Name;

            var user = await _userManager.FindByNameAsync(username);
            var roles = await _userManager.GetRolesAsync(user);
            var roleEntities = _roleManager.Roles.Where(r => roles.Contains(r.Name));

            var isAdmin = roleEntities.Any(r => r.IsAdmin);
            if (!isAdmin) // A role without admin access already can't reach this via the screens, but manual requests must also be blocked.
                return Unauthorized("You are not authorized to access this resource.");

            var result = await _endpointService.GetAccessibleMenuNamesAsync(username);
            return Ok(result);
        }
    }
}
