using WebAppAPI.Application.Abstractions.Services.Authentications;
using WebAppAPI.Application.Features.Auth.DTOs;

namespace WebAppAPI.Application.Abstractions.Services
{
    public interface IAuthService : IInternalAuthentication, IExternalAuthentication
    {
        Task PasswordResetAsync(string email, CancellationToken cancellationToken);
        Task<bool> VerifyResetTokenAsync(string resetToken, string userId);
        Task<IdentityCheckResultDto> IdentityCheckAsync(CancellationToken cancellationToken);
        Task LogoutAsync();
    }
}
