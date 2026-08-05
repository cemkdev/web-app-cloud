using MediatR;
using System.Text.Json.Serialization;
using WebAppAPI.Application.Features.Endpoints.DTOs;

namespace WebAppAPI.Application.Features.Endpoints.Commands.AssignRoleEndpoint
{
    public sealed class AssignRoleEndpointCommandRequest : IRequest<AssignRoleEndpointCommandResponse>
    {
        public required List<RolesEndpointsDto> RolesEndpoints { get; init; }

        [JsonIgnore]
        public Type? ApplicationType { get; set; }
    }
}
