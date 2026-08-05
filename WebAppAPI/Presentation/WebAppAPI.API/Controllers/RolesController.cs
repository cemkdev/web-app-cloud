using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppAPI.Application.Consts;
using WebAppAPI.Application.CustomAttributes;
using WebAppAPI.Application.Enums;
using WebAppAPI.Application.Features.Roles.Commands.CreateRole;
using WebAppAPI.Application.Features.Roles.Commands.DeleteRange;
using WebAppAPI.Application.Features.Roles.Commands.DeleteRole;
using WebAppAPI.Application.Features.Roles.Commands.UpdateRole;
using WebAppAPI.Application.Features.Roles.Queries.GetRoleById;
using WebAppAPI.Application.Features.Roles.Queries.GetRoles;
using WebAppAPI.Domain.Constants;

namespace WebAppAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
    public class RolesController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet("get-roles")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Roles, Definition = "Get Roles", ActionType = ActionType.Read, AdminOnly = true)]
        public async Task<ActionResult<List<GetRolesQueryResponse>>> GetRoles(CancellationToken cancellationToken)
        {
            List<GetRolesQueryResponse> response = await _mediator.Send(
                new GetRolesQueryRequest(),
                cancellationToken);

            return Ok(response);
        }

        [HttpGet("get-role-by-id/{id}")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Roles, Definition = "Get Role By Id", ActionType = ActionType.Read, AdminOnly = true)]
        public async Task<ActionResult<GetRoleByIdQueryResponse>> GetRoleById([FromRoute] string id, CancellationToken cancellationToken)
        {
            GetRoleByIdQueryResponse response = await _mediator.Send(
                new GetRoleByIdQueryRequest
                {
                    Id = id
                },
                cancellationToken);

            return Ok(response);
        }

        [HttpPost("create-role")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Roles, Definition = "Create Role", ActionType = ActionType.Write, AdminOnly = true)]
        public async Task<ActionResult<CreateRoleCommandResponse>> CreateRole([FromBody] CreateRoleCommandRequest request, CancellationToken cancellationToken)
        {
            CreateRoleCommandResponse response = await _mediator.Send(request, cancellationToken);

            return Ok(response);
        }

        [HttpPatch("update-role")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Roles, Definition = "Update Role", ActionType = ActionType.Update, AdminOnly = true)]
        public async Task<ActionResult<UpdateRoleCommandResponse>> UpdateRole([FromBody] UpdateRoleCommandRequest request, CancellationToken cancellationToken)
        {
            UpdateRoleCommandResponse response = await _mediator.Send(request, cancellationToken);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Roles, Definition = "Delete Role", ActionType = ActionType.Delete, AdminOnly = true)]
        public async Task<ActionResult<DeleteRoleCommandResponse>> DeleteRole([FromRoute] string id, CancellationToken cancellationToken)
        {
            DeleteRoleCommandResponse response = await _mediator.Send(
                new DeleteRoleCommandRequest
                {
                    Id = id
                },
                cancellationToken);

            return Ok(response);
        }

        [HttpPost("delete-range-role")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Roles, Definition = "Delete Range of Role", ActionType = ActionType.Delete, AdminOnly = true)]
        public async Task<ActionResult<DeleteRangeCommandResponse>> DeleteRange([FromBody] DeleteRangeCommandRequest request, CancellationToken cancellationToken)
        {
            DeleteRangeCommandResponse response = await _mediator.Send(request, cancellationToken);

            return Ok(response);
        }
    }
}
