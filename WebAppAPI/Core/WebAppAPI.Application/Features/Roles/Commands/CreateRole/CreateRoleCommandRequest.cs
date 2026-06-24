using MediatR;

namespace WebAppAPI.Application.Features.Roles.Commands.CreateRole
{
    public class CreateRoleCommandRequest : IRequest<CreateRoleCommandResponse>
    {
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
    }
}
