using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebAppAPI.Application.Abstractions.Token;
using WebAppAPI.Application.Options.Authentication;
using I = WebAppAPI.Domain.Entities.Identity;
using T = WebAppAPI.Application.DTOs;

namespace WebAppAPI.Infrastructure.Services.Token
{
    public class TokenHandler : ITokenHandler
    {
        private readonly TokenOptions _tokenOptions;
        private readonly TokenExpirationOptions _tokenExpirationOptions;
        readonly TokenValidationParameters _validationParameters;

        public TokenHandler(
            IOptions<TokenOptions> tokenOptions,
            IOptions<TokenExpirationOptions> tokenExpirationOptions,
            TokenValidationParameters validationParameters)
        {
            _tokenOptions = tokenOptions.Value;
            _tokenExpirationOptions = tokenExpirationOptions.Value;
            _validationParameters = validationParameters;
        }

        public T.Token CreateAccessToken(I.AppUser user, bool isFromRefreshToken = false)
        {
            T.Token token = new();

            // We are getting the symmetric key of the Security Key.
            SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_tokenOptions.SecurityKey));

            // We are creating the encrypted identity.
            SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);

            int configuredAccessTokenLifetime = _tokenExpirationOptions.AccessToken;
            //token.Expiration = DateTime.UtcNow.AddSeconds(configuredAccessTokenLifetime);

            TimeSpan defaultAccessTokenLifetime = TimeSpan.FromSeconds(configuredAccessTokenLifetime);
            DateTime now = DateTime.UtcNow;
            DateTime expiration;
            if (isFromRefreshToken)
            {
                if (user.RefreshTokenEndDate == null || user.RefreshTokenEndDate <= now)
                    throw new SecurityTokenExpiredException("Session expired.");
                var remaining = user.RefreshTokenEndDate.Value - now;

                // Make sure the token lifetime isn't longer than the refresh token lifetime.
                var finalLifetime = remaining < defaultAccessTokenLifetime ? remaining : defaultAccessTokenLifetime;
                expiration = now.AddSeconds(finalLifetime.TotalSeconds);
            }
            else
                expiration = now.AddSeconds(configuredAccessTokenLifetime);

            token.Expiration = expiration;

            JwtSecurityToken SecurityToken = new(
                audience: _tokenOptions.Audience,
                issuer: _tokenOptions.Issuer,
                expires: token.Expiration,
                notBefore: now,
                signingCredentials: signingCredentials,
                claims: new List<Claim> { new(ClaimTypes.Name, user.UserName) }
                );

            // Let's create an instance of the token generator class.
            JwtSecurityTokenHandler tokenHandler = new();
            token.AccessToken = tokenHandler.WriteToken(SecurityToken);

            return token;
        }

        public string CreateRefreshToken()
        {
            byte[] number = new byte[32];
            using RandomNumberGenerator random = RandomNumberGenerator.Create();
            random.GetBytes(number);

            return Convert.ToBase64String(number);
        }

        // TODO: Bu method async denmesine rağmen async değilmiş içi. İçinde await yokmuş. Önceki çalışmalarda chat gpt uyardı. Komple code refactor adımlarında bu ve bunun gibi hata sebebi olmayan ama doğru olmayan kullanımlarla ilgilenelim.
        public async Task<ClaimsPrincipal> ValidateAccessTokenAsync(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(accessToken, _validationParameters, out var validatedToken);
                return claimsPrincipal;
            }
            catch (SecurityTokenException ex)
            {
                throw new UnauthorizedAccessException("Invalid or expired token.", ex);
            }
        }

        public string? GetUsernameFromExpiredToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token); // It does not check for expiration; it only parses.
            var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            return usernameClaim?.Value;
        }
    }
}
