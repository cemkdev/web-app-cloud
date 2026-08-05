namespace WebAppAPI.Application.Features.Users.Queries.GetRolesByUserId
{
    public sealed class GetRolesByUserIdQueryResponse
    {
        public required string RoleId { get; init; }
        public required string RoleName { get; init; }
        public required bool IsAdmin { get; init; }
        public required bool IsAssigned { get; init; }
    }
}
