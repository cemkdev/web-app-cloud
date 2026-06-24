using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Auth.Queries.IdentityCheck
{
    public class IdentityCheckQueryHandler : IRequestHandler<IdentityCheckQueryRequest, IdentityCheckQueryResponse>
    {
        readonly IAuthService _authService;

        public IdentityCheckQueryHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<IdentityCheckQueryResponse> Handle(IdentityCheckQueryRequest request, CancellationToken cancellationToken)
        {
            var identityCheckResult = await _authService.IdentityCheckAsync();
            return new()
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
