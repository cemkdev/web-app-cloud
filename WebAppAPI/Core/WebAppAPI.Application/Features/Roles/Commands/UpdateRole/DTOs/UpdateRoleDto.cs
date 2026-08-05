namespace WebAppAPI.Application.Features.Roles.Commands.UpdateRole.DTOs
{
    public sealed class UpdateRoleDto
    {
        public required string Id { get; init; }
        public string? Name { get; init; }
        public bool? IsAdmin { get; init; }
    }
}
