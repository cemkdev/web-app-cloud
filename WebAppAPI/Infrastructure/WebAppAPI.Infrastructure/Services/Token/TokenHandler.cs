using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebAppAPI.Application.Abstractions.Token;
using WebAppAPI.Application.Features.Auth.DTOs;
using WebAppAPI.Application.Options.Authentication;
using WebAppAPI.Domain.Entities.Identity;

namespace WebAppAPI.Infrastructure.Services.Token
{
    public class TokenHandler(IOptions<TokenOptions> tokenOptions,
            IOptions<TokenExpirationOptions> tokenExpirationOptions,
            TokenValidationParameters validationParameters) : ITokenHandler
    {
        private readonly TokenOptions _tokenOptions = tokenOptions.Value;
        private readonly TokenExpirationOptions _tokenExpirationOptions = tokenExpirationOptions.Value;
        private readonly TokenValidationParameters _validationParameters = validationParameters;

        public AccessTokenResultDto CreateAccessToken(AppUser user, bool isFromRefreshToken = false)
        {
            ArgumentNullException.ThrowIfNull(user);

            DateTime now = DateTime.UtcNow;
            DateTime expiration = CalculateAccessTokenExpiration(user, now, isFromRefreshToken);

            string username = user.UserName ?? throw new SecurityTokenException("Token subject username is missing.");

            SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_tokenOptions.SecurityKey));
            SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken securityToken = new(
                audience: _tokenOptions.Audience,
                issuer: _tokenOptions.Issuer,
                expires: expiration,
                notBefore: now,
                signingCredentials: signingCredentials,
                claims: new List<Claim>
                {
                    new(ClaimTypes.Name, username)
                });

            string accessToken = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return new AccessTokenResultDto
            {
                AccessToken = accessToken,
                Expiration = expiration
            };
        }

        public string CreateRefreshToken()
        {
            byte[] randomBytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(randomBytes);
        }

        public ClaimsPrincipal ValidateAccessToken(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new UnauthorizedAccessException("Access token is missing.");

            try
            {
                return new JwtSecurityTokenHandler().ValidateToken(accessToken, _validationParameters, out _);
            }
            catch (SecurityTokenException ex)
            {
                throw new UnauthorizedAccessException("Invalid or expired token.", ex);
            }
        }

        public string? GetUsernameFromExpiredToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            JwtSecurityTokenHandler tokenHandler = new();

            if (!tokenHandler.CanReadToken(token))
                return null;

            try
            {
                JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(token);

                return jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        #region Helpers
        private DateTime CalculateAccessTokenExpiration(AppUser user, DateTime now, bool isFromRefreshToken)
        {
            int configuredAccessTokenLifetime = _tokenExpirationOptions.AccessToken;

            if (!isFromRefreshToken)
                return now.AddSeconds(configuredAccessTokenLifetime);

            if (user.RefreshTokenEndDate is null || user.RefreshTokenEndDate <= now)
                throw new SecurityTokenExpiredException("Session expired.");

            TimeSpan defaultAccessTokenLifetime = TimeSpan.FromSeconds(configuredAccessTokenLifetime);
            TimeSpan remainingRefreshTokenLifetime = user.RefreshTokenEndDate.Value - now;
            TimeSpan finalLifetime = remainingRefreshTokenLifetime < defaultAccessTokenLifetime
                ? remainingRefreshTokenLifetime
                : defaultAccessTokenLifetime;

            return now.Add(finalLifetime);
        }
        #endregion
    }
}
