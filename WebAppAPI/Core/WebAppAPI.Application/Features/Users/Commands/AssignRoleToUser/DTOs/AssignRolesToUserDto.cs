namespace WebAppAPI.Application.Features.Users.Commands.AssignRoleToUser.DTOs
{
    public sealed class AssignRolesToUserDto
    {
        public required string UserId { get; init; }
        public required IReadOnlyCollection<string> Roles { get; init; }
    }
}
