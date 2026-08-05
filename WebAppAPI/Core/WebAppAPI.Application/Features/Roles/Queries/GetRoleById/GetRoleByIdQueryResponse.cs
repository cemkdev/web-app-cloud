namespace WebAppAPI.Application.Features.Roles.Queries.GetRoleById
{
    public sealed class GetRoleByIdQueryResponse
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public bool IsAdmin { get; init; }
    }
}
