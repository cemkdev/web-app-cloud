namespace WebAppAPI.Application.DTOs.AuthorizationDefinitions
{
    public sealed class EndpointMenuDto
    {
        public required string Name { get; init; }
        public List<EndpointDefinitionDto> Endpoints { get; init; } = [];
    }
}
