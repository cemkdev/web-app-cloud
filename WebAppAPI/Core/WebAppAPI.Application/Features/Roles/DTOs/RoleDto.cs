namespace WebAppAPI.Application.Features.Roles.DTOs
{
    public sealed class RoleDto
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public bool IsAdmin { get; init; }
    }
}
