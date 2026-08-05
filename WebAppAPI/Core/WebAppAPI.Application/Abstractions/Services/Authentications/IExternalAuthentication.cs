using WebAppAPI.Application.Features.Auth.DTOs;

namespace WebAppAPI.Application.Abstractions.Services.Authentications
{
    public interface IExternalAuthentication
    {
        Task<AccessTokenResultDto> FacebookLoginAsync(string authToken, CancellationToken cancellationToken);
        Task<AccessTokenResultDto> GoogleLoginAsync(string idToken);
    }
}
