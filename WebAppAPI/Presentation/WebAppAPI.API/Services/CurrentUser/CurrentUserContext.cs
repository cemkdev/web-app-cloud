using System.Security.Claims;
using WebAppAPI.Application.Abstractions.CurrentUser;

namespace WebAppAPI.API.Services.CurrentUser
{
    public sealed class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
    {
        public string UserId => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Authenticated user id is not available.");
    }
}
