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
    public class RolesController : ControllerBase
    {
        readonly IMediator _mediator;

        public RolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("get-roles")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Roles, Definition = "Get Roles", ActionType = ActionType.Read, AdminOnly = true)]
        public async Task<IActionResult> GetRoles([FromQuery] GetRolesQueryRequest getRolesQueryRequest)
        {
            List<GetRolesQueryResponse> response = await _mediator.Send(getRolesQueryRequest);
            return Ok(response);
        }

        [HttpGet("get-role-by-id/{id}")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Roles, Definition = "Get Role By Id", ActionType = ActionType.Read, AdminOnly = true)]
        public async Task<IActionResult> GetRoleById([FromRoute] GetRoleByIdQueryRequest getRoleByIdQueryRequest)
        {
            GetRoleByIdQueryResponse response = await _mediator.Send(getRoleByIdQueryRequest);
            return Ok(response);
        }

        [HttpPost("create-role")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Roles, Definition = "Create Role", ActionType = ActionType.Write, AdminOnly = true)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommandRequest createRoleCommandRequest)
        {
            CreateRoleCommandResponse response = await _mediator.Send(createRoleCommandRequest);
            return Ok(response);
        }

        [HttpPut("update-role")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Roles, Definition = "Update Role", ActionType = ActionType.Update, AdminOnly = true)]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleCommandRequest updateRoleCommandRequest)
        {
            UpdateRoleCommandResponse response = await _mediator.Send(updateRoleCommandRequest);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Roles, Definition = "Delete Role", ActionType = ActionType.Delete, AdminOnly = true)]
        public async Task<IActionResult> DeleteRole([FromRoute] DeleteRoleCommandRequest deleteRoleCommandRequest)
        {
            DeleteRoleCommandResponse response = await _mediator.Send(deleteRoleCommandRequest);
            return Ok(response);
        }

        [HttpPost("delete-range-role")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Roles, Definition = "Delete Range of Role", ActionType = ActionType.Delete, AdminOnly = true)]
        public async Task<IActionResult> DeleteRange([FromBody] DeleteRangeCommandRequest deleteRangeCommandRequest)
        {
            DeleteRangeCommandResponse response = await _mediator.Send(deleteRangeCommandRequest);
            return Ok();
        }
    }
}
