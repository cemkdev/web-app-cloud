using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Endpoints.Commands.AssignRoleEndpoint
{
    public class AssignRoleEndpointCommandHandler : IRequestHandler<AssignRoleEndpointCommandRequest, AssignRoleEndpointCommandResponse>
    {
        readonly IEndpointService _endpointService;

        public AssignRoleEndpointCommandHandler(IEndpointService endpointService)
        {
            _endpointService = endpointService;
        }

        public async Task<AssignRoleEndpointCommandResponse> Handle(AssignRoleEndpointCommandRequest request, CancellationToken cancellationToken)
        {
            await _endpointService.AssignRoleToEndpointsAsync(request.RolesEndpoints, request.Type);
            return new();
        }
    }
}
