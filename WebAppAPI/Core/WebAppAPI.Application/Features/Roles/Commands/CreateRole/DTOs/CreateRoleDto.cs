namespace WebAppAPI.Application.Features.Roles.Commands.CreateRole.DTOs
{
    public sealed class CreateRoleDto
    {
        public required string Name { get; init; }
        public bool IsAdmin { get; init; }
    }
}
