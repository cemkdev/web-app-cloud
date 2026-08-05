namespace WebAppAPI.Application.Features.Roles.Queries.GetRoles
{
    public sealed class GetRolesQueryResponse
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public bool IsAdmin { get; init; }

    }
}
