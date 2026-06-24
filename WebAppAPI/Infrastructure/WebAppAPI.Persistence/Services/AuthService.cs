using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Json;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Abstractions.Token;
using WebAppAPI.Application.DTOs;
using WebAppAPI.Application.DTOs.Facebook;
using WebAppAPI.Application.Exceptions;
using WebAppAPI.Application.Helpers;
using WebAppAPI.Application.Options.Authentication;
using U = WebAppAPI.Domain.Entities.Identity;

namespace WebAppAPI.Persistence.Services
{
    public class AuthService(
        IHttpClientFactory httpClientFactory,
        UserManager<U.AppUser> userManager,
        ITokenHandler tokenHandler,
        SignInManager<U.AppUser> signInManager,
        IUserService userService,
        IPermissionService permissionService,
        IMailService mailService,
        IHttpContextAccessor httpContextAccessor,
        IOptions<TokenExpirationOptions> tokenExpirationOptions,
        IOptions<AuthCookieOptions> authCookieOptions,
        IOptions<ExternalLoginOptions> externalLoginOptions) : IAuthService
    {
        readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        readonly UserManager<U.AppUser> _userManager = userManager;
        readonly ITokenHandler _tokenHandler = tokenHandler;
        readonly SignInManager<U.AppUser> _signInManager = signInManager;
        readonly IUserService _userService = userService;
        readonly IPermissionService _permissionService = permissionService;
        readonly IMailService _mailService = mailService;
        IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly TokenExpirationOptions _tokenExpirationOptions = tokenExpirationOptions.Value;
        private readonly AuthCookieOptions _authCookieOptions = authCookieOptions.Value;
        private readonly ExternalLoginOptions _externalLoginOptions = externalLoginOptions.Value;

        #region Internal Login
        public async Task<Token> LoginAsync(string usernameOrEmail, string password)
        {
            U.AppUser user = await _userManager.FindByNameAsync(usernameOrEmail);
            if (user == null)
                user = await _userManager.FindByEmailAsync(usernameOrEmail);

            if (user == null)
                throw new NotFoundUserException();

            if (await _userManager.IsLockedOutAsync(user))
                throw new UserLockedOutException();

            SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (result.Succeeded)
            {
                await _userManager.ResetAccessFailedCountAsync(user);

                Token token = _tokenHandler.CreateAccessToken(user);
                string refreshToken = _tokenHandler.CreateRefreshToken();
                await _userService.UpdateRefreshTokenAsync(user, refreshToken, _tokenExpirationOptions.RefreshToken);

                SetHttpOnlyAccessTokenCookie(token);

                return token;
            }
            else if (result.IsLockedOut)
            {
                throw new UserLockedOutException();
            }
            else
            {
                await _userManager.AccessFailedAsync(user);
                throw new AuthenticationFailedException();
            }
        }
        #endregion

        #region External Login
        public async Task<Token> FacebookLoginAsync(string authToken)
        {
            // If Facebook external login settings are not configured, skip external login.
            if (string.IsNullOrWhiteSpace(_externalLoginOptions.Facebook.ClientId) ||
                string.IsNullOrWhiteSpace(_externalLoginOptions.Facebook.ClientSecret))
            {
                return null!;
            }

            string accessTokenResponse = await _httpClient.GetStringAsync($"https://graph.facebook.com/oauth/access_token?client_id={_externalLoginOptions.Facebook.ClientId}&client_secret={_externalLoginOptions.Facebook.ClientSecret}&grant_type=client_credentials");

            FacebookAccessTokenResponse? facebookAccessTokenResponse = JsonSerializer.Deserialize<FacebookAccessTokenResponse>(accessTokenResponse);
            string userAccessTokenValidation = await _httpClient.GetStringAsync($"https://graph.facebook.com/debug_token?input_token={authToken}&access_token={facebookAccessTokenResponse?.AccessToken}");

            FacebookUserAccessTokenValidation? validation = JsonSerializer.Deserialize<FacebookUserAccessTokenValidation>(userAccessTokenValidation);

            if (validation?.Data.IsValid != null)
            {
                string userInfoResponse = await _httpClient.GetStringAsync($"https://graph.facebook.com/me?fields=first_name,last_name,name,email&access_token={authToken}");

                FacebookUserInfoResponse? userInfo = JsonSerializer.Deserialize<FacebookUserInfoResponse>(userInfoResponse);

                ExternalLoginInfo externalLoginInfo = new()
                {
                    Email = userInfo?.Email,
                    FirstName = userInfo?.FirstName,
                    LastName = userInfo?.LastName,
                    FullName = userInfo?.FullName
                };

                var info = new UserLoginInfo("FACEBOOK", validation.Data.UserId, "FACEBOOK");
                U.AppUser user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

                return await CreateUserExternalAsync(user, externalLoginInfo, info);
            }
            throw new Exception("Invalid external authentication.");
        }

        public async Task<Token> GoogleLoginAsync(string idToken)
        {
            // If Google external login settings are not configured, skip external login.
            if (string.IsNullOrWhiteSpace(_externalLoginOptions.Google.ClientId))
            {
                return null!;
            }

            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { _externalLoginOptions.Google.ClientId }
            };

            GoogleJsonWebSignature.Payload? payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            ExternalLoginInfo externalLoginInfo = new()
            {
                Email = payload?.Email,
                FirstName = payload?.GivenName,
                LastName = payload?.FamilyName,
                FullName = payload?.Name
            };

            var info = new UserLoginInfo("GOOGLE", payload.Subject, "GOOGLE");
            U.AppUser user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            return await CreateUserExternalAsync(user, externalLoginInfo, info);
        }
        #endregion

        #region IdentityCheck
        public async Task<IdentityCheckDto> IdentityCheckAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null)
                throw new NotFoundUserException();

            if (user.Identity == null)
                throw new NotFoundUserException();

            if (!user.Identity.IsAuthenticated)
            {
                return new IdentityCheckDto
                {
                    UserId = null,
                    Username = null,
                    IsAuthenticated = false,
                    Expiration = DateTime.MinValue,
                    RefreshBeforeTime = null,
                    IsAdmin = false
                };
            }
            var username = user.Identity.Name;
            var userFromDB = await _userManager.FindByNameAsync(username);
            var isAdmin = await _permissionService.HasAdminAccessAsync(username);

            var expirationClaim = user.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            DateTime expirationDate = DateTime.MinValue;

            if (expirationClaim != null && long.TryParse(expirationClaim, out var expUnix))
                expirationDate = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;

            string refreshBeforeTime = _tokenExpirationOptions.RefreshBeforeTime.ToString();

            return new IdentityCheckDto
            {
                UserId = userFromDB.Id,
                Username = username,
                IsAuthenticated = true,
                Expiration = expirationDate,
                RefreshBeforeTime = refreshBeforeTime,
                IsAdmin = isAdmin
            };
        }
        #endregion

        #region PasswordReset
        public async Task PasswordResetAsync(string email)
        {
            U.AppUser user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                resetToken = resetToken.UrlEncode();
                await _mailService.SendPasswordResetMailAsync(email, user.Id, user.FirstName, resetToken);
            }
        }

        public async Task<bool> VerifyResetTokenAsync(string resetToken, string userId)
        {
            U.AppUser user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                try
                {
                    resetToken = resetToken.UrlDecode();
                    return await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", resetToken);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }
        #endregion

        public async Task LogoutAsync()
        {
            var accessToken = _httpContextAccessor.HttpContext?.Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(accessToken))
                throw new AuthenticationFailedException();

            // Extract username from an expired access token for logout cleanup.
            var username = _tokenHandler.GetUsernameFromExpiredToken(accessToken);
            if (string.IsNullOrEmpty(username))
                throw new AuthenticationFailedException("User info could not be extracted from token.");

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
                throw new NotFoundUserException("User not found during logout.");

            _httpContextAccessor.HttpContext.Response.Cookies.Delete("accessToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = _authCookieOptions.Secure,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });
            await _userService.UpdateRefreshTokenAsync(user, null, 0, false, true);
        }

        #region Helpers
        async Task<Token> CreateUserExternalAsync(U.AppUser user, ExternalLoginInfo externalLoginInfo, UserLoginInfo info)
        {
            bool result = user != null;
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(externalLoginInfo.Email);
                if (user == null)
                {
                    user = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Email = externalLoginInfo.Email,
                        UserName = externalLoginInfo.Email,
                        FirstName = externalLoginInfo.FirstName,
                        LastName = externalLoginInfo.LastName,
                        FullName = externalLoginInfo.FullName
                    };
                    var identityResult = await _userManager.CreateAsync(user);
                    result = identityResult.Succeeded;
                }
            }

            if (result)
            {
                if (await _userManager.IsLockedOutAsync(user))
                    throw new UserLockedOutException();

                await _userManager.AddLoginAsync(user, info);

                Token token = _tokenHandler.CreateAccessToken(user);
                string refreshToken = _tokenHandler.CreateRefreshToken();
                await _userService.UpdateRefreshTokenAsync(user, refreshToken, _tokenExpirationOptions.RefreshToken);

                SetHttpOnlyAccessTokenCookie(token);

                return token;
            }
            throw new Exception("Invalid external authentication.");
        }

        public async Task<Token> RefreshTokenLoginAsync()
        {
            var accessToken = _httpContextAccessor.HttpContext?.Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(accessToken))
                throw new AuthenticationFailedException();

            var claimsPrincipal = await _tokenHandler.ValidateAccessTokenAsync(accessToken);
            string username = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;

            U.AppUser? user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user != null && user?.RefreshTokenEndDate > DateTime.UtcNow)
            {
                Token token = _tokenHandler.CreateAccessToken(user, true);
                string newRefreshToken = _tokenHandler.CreateRefreshToken();
                await _userService.UpdateRefreshTokenAsync(user, newRefreshToken, _tokenExpirationOptions.RefreshToken, true);
                SetHttpOnlyAccessTokenCookie(token);

                return token;
            }
            else
                throw new AuthenticationFailedException();
        }

        private void SetHttpOnlyAccessTokenCookie(Token token)
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Append("accessToken", token.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = _authCookieOptions.Secure,
                Expires = token.Expiration,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });
        }

        class ExternalLoginInfo
        {
            public string? Email { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? FullName { get; set; }
        }
        #endregion
    }
}
