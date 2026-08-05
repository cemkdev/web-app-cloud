using WebAppAPI.Application.Enums;

namespace WebAppAPI.Application.DTOs.AuthorizationDefinitions
{
    public sealed class EndpointDefinitionDto
    {
        public ActionType ActionType { get; init; }
        public required string HttpType { get; init; }
        public required string Definition { get; init; }
        public required string Code { get; init; }
        public bool AdminOnly { get; init; }
    }
}
