using WebAppAPI.Application.Features.Auth.DTOs;

namespace WebAppAPI.Application.Abstractions.Services.Authentications
{
    public interface IInternalAuthentication
    {
        Task<AccessTokenResultDto> LoginAsync(string usernameOrEmail, string password);
        Task<AccessTokenResultDto> RefreshTokenLoginAsync();
    }
}
