using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Endpoints.Queries.HasAccessToMenu
{
    public sealed class HasAccessToMenuQueryHandler(IEndpointService endpointService) : IRequestHandler<HasAccessToMenuQueryRequest, HasAccessToMenuQueryResponse>
    {
        public async Task<HasAccessToMenuQueryResponse> Handle(HasAccessToMenuQueryRequest request, CancellationToken cancellationToken)
        {
            bool hasAccess = await endpointService.HasAccessToMenuAsync(
                request.Username,
                request.MenuName,
                cancellationToken);

            return new HasAccessToMenuQueryResponse
            {
                HasAccess = hasAccess
            };
        }
    }
}
