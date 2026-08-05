namespace WebAppAPI.Application.Features.Endpoints.DTOs
{
    public sealed class RolesEndpointsDto
    {
        public required string RoleId { get; init; }
        public required IReadOnlyList<RoleEndpoint> RoleEndpoints { get; init; }
    }

    public sealed class RoleEndpoint
    {
        public required string MenuName { get; init; }
        public required string EndpointCode { get; init; }
        public required bool IsAuthorized { get; init; }
    }
}
