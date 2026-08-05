using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Endpoints.Commands.AssignRoleEndpoint
{
    public class AssignRoleEndpointCommandHandler(IEndpointService endpointService) : IRequestHandler<AssignRoleEndpointCommandRequest, AssignRoleEndpointCommandResponse>
    {
        public async Task<AssignRoleEndpointCommandResponse> Handle(AssignRoleEndpointCommandRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request.ApplicationType);

            await endpointService.AssignRoleToEndpointsAsync(
                request.RolesEndpoints,
                request.ApplicationType,
                cancellationToken);

            return new AssignRoleEndpointCommandResponse();
        }
    }
}
