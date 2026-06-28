using WebAppAPI.Application.Features.Auth.DTOs;

namespace WebAppAPI.Application.Abstractions.Services.Authentications
{
    public interface IExternalAuthentication
    {
        Task<AccessTokenResultDto> FacebookLoginAsync(string authToken);
        Task<AccessTokenResultDto> GoogleLoginAsync(string idToken);
    }
}
