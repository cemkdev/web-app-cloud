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
    public class UsersController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet("get-all-users")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Users, Definition = "Get All Users", ActionType = ActionType.Read, AdminOnly = true)]
        public async Task<ActionResult<GetAllUsersQueryResponse>> GetAllUsers([FromQuery] GetAllUsersQueryRequest request, CancellationToken cancellationToken)
        {
            GetAllUsersQueryResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("create-user")]
        public async Task<ActionResult<CreateUserCommandResponse>> CreateUser([FromBody] CreateUserCommandRequest request, CancellationToken cancellationToken)
        {
            CreateUserCommandResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("update-password")]
        public async Task<ActionResult<UpdatePasswordCommandResponse>> UpdatePassword([FromBody] UpdatePasswordCommandRequest request, CancellationToken cancellationToken)
        {
            UpdatePasswordCommandResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("get-roles-by-userid/{userId}")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        //[AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Users, Definition = "Get Roles By UserId", ActionType = ActionType.Read, AdminOnly = true)]
        public async Task<ActionResult<List<GetRolesByUserIdQueryResponse>>> GetRolesByUserId([FromRoute] string userId, CancellationToken cancellationToken)
        {
            List<GetRolesByUserIdQueryResponse> response = await _mediator.Send(
                new GetRolesByUserIdQueryRequest
                {
                    UserId = userId
                },
                cancellationToken);

            return Ok(response);
        }

        [HttpPost("assign-role-to-user")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Users, Definition = "Assign Role To User", ActionType = ActionType.Write, AdminOnly = true)]
        public async Task<ActionResult<AssignRoleToUserCommandResponse>> AssignRoleToUser([FromBody] AssignRoleToUserCommandRequest request, CancellationToken cancellationToken)
        {
            AssignRoleToUserCommandResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }
    }
}
