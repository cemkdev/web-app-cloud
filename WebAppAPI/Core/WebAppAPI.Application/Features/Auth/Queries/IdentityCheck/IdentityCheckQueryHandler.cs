using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Features.Auth.DTOs;

namespace WebAppAPI.Application.Features.Auth.Queries.IdentityCheck
{
    public sealed class IdentityCheckQueryHandler(IAuthService authService) : IRequestHandler<IdentityCheckQueryRequest, IdentityCheckQueryResponse>
    {
        public async Task<IdentityCheckQueryResponse> Handle(IdentityCheckQueryRequest request, CancellationToken cancellationToken)
        {
            IdentityCheckResultDto identityCheckResult = await authService.IdentityCheckAsync();

            return new IdentityCheckQueryResponse
            {
                UserId = identityCheckResult.UserId,
                Username = identityCheckResult.Username,
                IsAuthenticated = identityCheckResult.IsAuthenticated,
                Expiration = identityCheckResult.Expiration,
                RefreshBeforeTime = identityCheckResult.RefreshBeforeTime,
                IsAdmin = identityCheckResult.IsAdmin
            };
        }
    }
}
