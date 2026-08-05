using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppAPI.Application.Consts;
using WebAppAPI.Application.CustomAttributes;
using WebAppAPI.Application.Enums;
using WebAppAPI.Application.Features.Endpoints.Commands.AssignRoleEndpoint;
using WebAppAPI.Application.Features.Endpoints.Queries.GetAccessibleMenuNames;
using WebAppAPI.Application.Features.Endpoints.Queries.GetCurrentUserRoleEndpoints;
using WebAppAPI.Application.Features.Endpoints.Queries.GetRolesEndpoints;
using WebAppAPI.Application.Features.Endpoints.Queries.HasAccessToMenu;
using WebAppAPI.Domain.Constants;

namespace WebAppAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
    public class EndpointsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet("get-roles-endpoints")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Endpoints, Definition = "Get Roles and Endpoints", ActionType = ActionType.Read, AdminOnly = true)]
        public async Task<ActionResult<GetRolesEndpointsQueryResponse>> GetRolesEndpoints(CancellationToken cancellationToken)
        {
            GetRolesEndpointsQueryResponse response = await _mediator.Send(new GetRolesEndpointsQueryRequest(), cancellationToken);

            return Ok(response);
        }

        [HttpGet("get-current-user-role-endpoints")]
        public async Task<ActionResult<GetCurrentUserRoleEndpointsQueryResponse>> GetCurrentUserRoleEndpoints(CancellationToken cancellationToken)
        {
            string username = User.Identity?.Name ?? throw new UnauthorizedAccessException();

            GetCurrentUserRoleEndpointsQueryResponse response = await _mediator.Send(
                new GetCurrentUserRoleEndpointsQueryRequest
                {
                    Username = username
                },
                cancellationToken);

            return Ok(response);
        }

        [HttpPost("assign-role-endpoints")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Endpoints, Definition = "Assign Roles to Endpoints", ActionType = ActionType.Write, AdminOnly = true)]
        public async Task<ActionResult<AssignRoleEndpointCommandResponse>> AssignRoleEndpoints([FromBody] AssignRoleEndpointCommandRequest request, CancellationToken cancellationToken)
        {
            request.ApplicationType = typeof(Program);

            AssignRoleEndpointCommandResponse response = await _mediator.Send(request, cancellationToken);

            return Ok(response);
        }

        [HttpGet("has-access")]
        public async Task<ActionResult<HasAccessToMenuQueryResponse>> HasAccess([FromQuery] string menuName, CancellationToken cancellationToken)
        {
            string? username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized();

            HasAccessToMenuQueryResponse response = await _mediator.Send(
                new HasAccessToMenuQueryRequest
                {
                    Username = username,
                    MenuName = menuName
                },
                cancellationToken);

            return Ok(response);
        }

        [HttpGet("accessible-menus")]
        public async Task<ActionResult<List<string>>> GetAccessibleMenuNames(CancellationToken cancellationToken)
        {
            string? username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized();

            List<string> response = await _mediator.Send(
                new GetAccessibleMenuNamesQueryRequest
                {
                    Username = username
                },
                cancellationToken);

            return Ok(response);
        }
    }
}
