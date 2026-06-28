using System.Security.Claims;
using WebAppAPI.Application.Features.Auth.DTOs;
using WebAppAPI.Domain.Entities.Identity;

namespace WebAppAPI.Application.Abstractions.Token
{
    public interface ITokenHandler
    {
        AccessTokenResultDto CreateAccessToken(AppUser appUser, bool isFromRefreshToken = false);
        string CreateRefreshToken();
        ClaimsPrincipal ValidateAccessToken(string accessToken);

        string? GetUsernameFromExpiredToken(string token);
    }
}
