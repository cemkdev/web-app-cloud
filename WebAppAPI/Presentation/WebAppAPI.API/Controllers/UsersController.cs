using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppAPI.Application.Consts;
using WebAppAPI.Application.CustomAttributes;
using WebAppAPI.Application.Enums;
using WebAppAPI.Application.Features.Users.Commands.AssignRoleToUser;
using WebAppAPI.Application.Features.Users.Commands.CreateUser;
using WebAppAPI.Application.Features.Users.Commands.UpdatePassword;
using WebAppAPI.Application.Features.Users.Queries.GetAllUsers;
using WebAppAPI.Application.Features.Users.Queries.GetRolesByUserId;
using WebAppAPI.Domain.Constants;

namespace WebAppAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("get-all-users")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Users, Definition = "Get All Users", ActionType = ActionType.Read, AdminOnly = true)]
        public async Task<IActionResult> GetAllUsers([FromQuery] GetAllUsersQueryRequest getAllUsersQueryRequest)
        {
            GetAllUsersQueryResponse response = await _mediator.Send(getAllUsersQueryRequest);
            return Ok(response);
        }

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser(CreateUserCommandRequest createUserCommandRequest)
        {
            CreateUserCommandResponse response = await _mediator.Send(createUserCommandRequest);
            return Ok(response);
        }

        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordCommandRequest updatePasswordCommandRequest)
        {
            UpdatePasswordCommandResponse response = await _mediator.Send(updatePasswordCommandRequest);
            return Ok(response);
        }

        [HttpGet("get-roles-by-userid/{userId}")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        //[AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Users, Definition = "Get Roles By UserId", ActionType = ActionType.Read, AdminOnly = true)]
        public async Task<IActionResult> GetRolesByUserId([FromRoute] GetRolesByUserIdQueryRequest getRolesByUserIdQueryRequest)
        {
            List<GetRolesByUserIdQueryResponse> response = await _mediator.Send(getRolesByUserIdQueryRequest);
            return Ok(response);
        }

        [HttpPost("assign-role-to-user")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Users, Definition = "Assign Role To User", ActionType = ActionType.Write, AdminOnly = true)]
        public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleToUserCommandRequest assignRoleToUserCommandRequest)
        {
            AssignRoleToUserCommandResponse response = await _mediator.Send(assignRoleToUserCommandRequest);
            return Ok(response);
        }
    }
}
