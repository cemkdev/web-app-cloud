using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Json;
using WebAppAPI.Application.Abstractions.Messaging;
using WebAppAPI.Application.Abstractions.Messaging.Messages;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Abstractions.Token;
using WebAppAPI.Application.Exceptions;
using WebAppAPI.Application.Features.Auth.Commands.FacebookLogin.DTOs;
using WebAppAPI.Application.Features.Auth.DTOs;
using WebAppAPI.Application.Helpers;
using WebAppAPI.Application.Options.Authentication;
using WebAppAPI.Application.Options.IdentityTokens;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Domain.Entities.Identity;

namespace WebAppAPI.Persistence.Services
{
    public sealed class AuthService(
        IHttpClientFactory httpClientFactory,
        UserManager<AppUser> userManager,
        ITokenHandler tokenHandler,
        SignInManager<AppUser> signInManager,
        IPermissionService permissionService,
        IOutboxWriter outboxWriter,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IOptions<TokenExpirationOptions> tokenExpirationOptions,
        IOptions<IdentityTokenOptions> identityTokenOptions,
        IOptions<AuthCookieOptions> authCookieOptions,
        IOptions<ExternalLoginOptions> externalLoginOptions) : IAuthService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly ITokenHandler _tokenHandler = tokenHandler;
        private readonly SignInManager<AppUser> _signInManager = signInManager;
        private readonly IPermissionService _permissionService = permissionService;
        private readonly IOutboxWriter _outboxWriter = outboxWriter;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly TokenExpirationOptions _tokenExpirationOptions = tokenExpirationOptions.Value;
        private readonly IdentityTokenOptions _identityTokenOptions = identityTokenOptions.Value;
        private readonly AuthCookieOptions _authCookieOptions = authCookieOptions.Value;
        private readonly ExternalLoginOptions _externalLoginOptions = externalLoginOptions.Value;

        private const string AccessTokenCookieName = "accessToken";

        #region Internal Login
        public async Task<AccessTokenResultDto> LoginAsync(string usernameOrEmail, string password)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
                throw new AuthenticationFailedException();

            AppUser? user = await _userManager.FindByNameAsync(usernameOrEmail);

            if (user is null)
                user = await _userManager.FindByEmailAsync(usernameOrEmail);

            if (user is null)
                throw new NotFoundUserException();

            if (await _userManager.IsLockedOutAsync(user))
                throw new UserLockedOutException();

            SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, password, false);

            if (result.IsLockedOut)
                throw new UserLockedOutException();

            if (!result.Succeeded)
            {
                await _userManager.AccessFailedAsync(user);
                throw new AuthenticationFailedException();
            }

            IdentityResult resetAccessFailedResult = await _userManager.ResetAccessFailedCountAsync(user);

            if (!resetAccessFailedResult.Succeeded)
                throw new AuthenticationFailedException();

            AccessTokenResultDto token = _tokenHandler.CreateAccessToken(user);
            string refreshToken = _tokenHandler.CreateRefreshToken();

            await UpdateRefreshTokenAsync(user, refreshToken);

            SetAccessTokenCookie(token);

            return token;
        }

        public async Task<AccessTokenResultDto> RefreshTokenLoginAsync()
        {
            HttpContext httpContext = GetRequiredHttpContext();

            string? accessToken = httpContext.Request.Cookies[AccessTokenCookieName];

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new AuthenticationFailedException();

            ClaimsPrincipal claimsPrincipal = _tokenHandler.ValidateAccessToken(accessToken);

            string? username = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(username))
                throw new AuthenticationFailedException("User info could not be extracted from token.");

            AppUser? user = await _userManager.FindByNameAsync(username);

            if (user is null || user.RefreshTokenEndDate is null || user.RefreshTokenEndDate <= DateTime.UtcNow)
                throw new AuthenticationFailedException();

            AccessTokenResultDto token = _tokenHandler.CreateAccessToken(user, true);
            string newRefreshToken = _tokenHandler.CreateRefreshToken();

            await UpdateRefreshTokenAsync(
                user,
                newRefreshToken,
                isSilentRefresh: true);

            SetAccessTokenCookie(token);

            return token;
        }
        #endregion

        #region External Login
        public async Task<AccessTokenResultDto> FacebookLoginAsync(string authToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(authToken))
                throw new AuthenticationFailedException();

            // If Facebook external login settings are not configured.
            // External login requires configured provider settings.
            if (string.IsNullOrWhiteSpace(_externalLoginOptions.Facebook.ClientId) ||
                string.IsNullOrWhiteSpace(_externalLoginOptions.Facebook.ClientSecret))
                throw new AuthenticationFailedException("Facebook login is not configured.");

            string accessTokenResponse = await _httpClient.GetStringAsync(
                $"https://graph.facebook.com/oauth/access_token?client_id={_externalLoginOptions.Facebook.ClientId}&client_secret={_externalLoginOptions.Facebook.ClientSecret}&grant_type=client_credentials",
                cancellationToken);

            AccessTokenResponse? facebookAccessTokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(accessTokenResponse);

            if (string.IsNullOrWhiteSpace(facebookAccessTokenResponse?.AccessToken))
                throw new AuthenticationFailedException("Invalid external authentication.");

            string userAccessTokenValidation = await _httpClient.GetStringAsync(
                $"https://graph.facebook.com/debug_token?input_token={authToken}&access_token={facebookAccessTokenResponse.AccessToken}",
                cancellationToken);

            TokenValidationResponse? validation = JsonSerializer.Deserialize<TokenValidationResponse>(userAccessTokenValidation);

            if (validation?.Data?.IsValid != true || string.IsNullOrWhiteSpace(validation.Data.UserId))
                throw new AuthenticationFailedException("Invalid external authentication.");

            string userInfoResponse = await _httpClient.GetStringAsync(
                $"https://graph.facebook.com/me?fields=first_name,last_name,name,email&access_token={authToken}",
                cancellationToken);

            UserInfoResponse? userInfo = JsonSerializer.Deserialize<UserInfoResponse>(userInfoResponse);

            ExternalLoginInfo externalLoginInfo = new()
            {
                Email = userInfo?.Email,
                FirstName = userInfo?.FirstName,
                LastName = userInfo?.LastName,
                FullName = userInfo?.FullName
            };

            UserLoginInfo loginInfo = new("FACEBOOK", validation.Data.UserId, "FACEBOOK");
            AppUser? user = await _userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);

            return await CreateUserExternalAsync(user, externalLoginInfo, loginInfo);
        }

        public async Task<AccessTokenResultDto> GoogleLoginAsync(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken))
                throw new AuthenticationFailedException();

            // If Google external login settings are not configured.
            // External login requires configured provider settings.
            if (string.IsNullOrWhiteSpace(_externalLoginOptions.Google.ClientId))
                throw new AuthenticationFailedException("Google login is not configured.");

            GoogleJsonWebSignature.ValidationSettings settings = new()
            {
                Audience = new List<string> { _externalLoginOptions.Google.ClientId }
            };

            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            ExternalLoginInfo externalLoginInfo = new()
            {
                Email = payload.Email,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                FullName = payload.Name
            };

            UserLoginInfo loginInfo = new("GOOGLE", payload.Subject, "GOOGLE");
            AppUser? user = await _userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);

            return await CreateUserExternalAsync(user, externalLoginInfo, loginInfo);
        }
        #endregion

        #region IdentityCheck
        public async Task<IdentityCheckResultDto> IdentityCheckAsync(CancellationToken cancellationToken)
        {
            HttpContext httpContext = GetRequiredHttpContext();

            ClaimsPrincipal principal = httpContext.User;

            if (principal.Identity?.IsAuthenticated != true)
                throw new AuthenticationFailedException();

            string? username = principal.Identity.Name;

            if (string.IsNullOrWhiteSpace(username))
                throw new AuthenticationFailedException("User info could not be extracted from token.");

            AppUser? user = await _userManager.FindByNameAsync(username);

            if (user is null)
                throw new NotFoundUserException();

            bool isAdmin = await _permissionService.HasAdminAccessAsync(username, cancellationToken);

            DateTime expirationDate = GetAccessTokenExpiration(principal);

            return new IdentityCheckResultDto
            {
                UserId = user.Id,
                Username = username,
                IsAuthenticated = true,
                Expiration = expirationDate,
                RefreshBeforeTime = _tokenExpirationOptions.RefreshBeforeTime.ToString(),
                IsAdmin = isAdmin
            };
        }
        #endregion

        #region PasswordReset
        public async Task PasswordResetAsync(string email, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(email))
                return;

            cancellationToken.ThrowIfCancellationRequested();

            AppUser? user = await _userManager.FindByEmailAsync(email);

            if (user is null)
                return;

            cancellationToken.ThrowIfCancellationRequested();

            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            resetToken = resetToken.UrlEncode();

            PasswordResetMailMessage mailMessage = new()
            {
                Recipient = email,
                UserId = user.Id,
                FirstName = user.FirstName,
                ResetToken = resetToken
            };

            Guid requestId = Guid.NewGuid();

            await _outboxWriter.EnqueueAsync(
                OutboxMessageTypes.PasswordResetMail,
                mailMessage,
                $"{OutboxMessageTypes.PasswordResetMail}:{requestId}",
                expiresAt: DateTime.UtcNow.AddMinutes(_identityTokenOptions.LifetimeMinutes),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> VerifyResetTokenAsync(string resetToken, string userId)
        {
            if (string.IsNullOrWhiteSpace(resetToken) || string.IsNullOrWhiteSpace(userId))
                return false;

            AppUser? user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                return false;

            try
            {
                string decodedResetToken = resetToken.UrlDecode();

                return await _userManager.VerifyUserTokenAsync(
                    user,
                    _userManager.Options.Tokens.PasswordResetTokenProvider,
                    "ResetPassword",
                    decodedResetToken);
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Logout
        public async Task LogoutAsync()
        {
            HttpContext httpContext = GetRequiredHttpContext();

            string? accessToken = httpContext.Request.Cookies[AccessTokenCookieName];

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new AuthenticationFailedException();

            string? expiredTokenUsername = _tokenHandler.GetUsernameFromExpiredToken(accessToken);

            if (string.IsNullOrWhiteSpace(expiredTokenUsername))
                throw new AuthenticationFailedException("User info could not be extracted from token.");

            AppUser? user = await _userManager.FindByNameAsync(expiredTokenUsername);

            if (user is null)
                throw new NotFoundUserException("User not found during logout.");

            await ClearRefreshTokenAsync(user);

            DeleteAccessTokenCookie();
        }
        #endregion

        #region Helpers
        private async Task<AccessTokenResultDto> CreateUserExternalAsync(AppUser? user, ExternalLoginInfo externalLoginInfo, UserLoginInfo loginInfo)
        {
            if (string.IsNullOrWhiteSpace(externalLoginInfo.Email))
                throw new AuthenticationFailedException("External login email is missing.");

            bool userFoundByExternalLogin = user is not null;

            user ??= await _userManager.FindByEmailAsync(externalLoginInfo.Email);

            if (user is null)
            {
                user = new AppUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = externalLoginInfo.Email,
                    UserName = externalLoginInfo.Email,
                    FirstName = externalLoginInfo.FirstName ?? string.Empty,
                    LastName = externalLoginInfo.LastName ?? string.Empty,
                    FullName = externalLoginInfo.FullName ?? externalLoginInfo.Email
                };

                IdentityResult createUserResult = await _userManager.CreateAsync(user);

                if (!createUserResult.Succeeded)
                    throw new AuthenticationFailedException("Invalid external authentication.");
            }

            if (await _userManager.IsLockedOutAsync(user))
                throw new UserLockedOutException();

            if (!userFoundByExternalLogin)
            {
                IdentityResult addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);

                if (!addLoginResult.Succeeded)
                    throw new AuthenticationFailedException("Invalid external authentication.");
            }

            AccessTokenResultDto token = _tokenHandler.CreateAccessToken(user);
            string refreshToken = _tokenHandler.CreateRefreshToken();

            await UpdateRefreshTokenAsync(user, refreshToken);

            SetAccessTokenCookie(token);

            return token;
        }

        private void SetAccessTokenCookie(AccessTokenResultDto token)
        {
            HttpContext httpContext = GetRequiredHttpContext();

            httpContext.Response.Cookies.Append(
                AccessTokenCookieName,
                token.AccessToken,
                BuildAccessTokenCookieOptions(token.Expiration));
        }

        private async Task UpdateRefreshTokenAsync(AppUser user, string refreshToken, bool isSilentRefresh = false)
        {
            ArgumentNullException.ThrowIfNull(user);

            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentException("Refresh token must be provided.", nameof(refreshToken));

            user.RefreshToken = refreshToken;

            if (!isSilentRefresh)
            {
                user.RefreshTokenEndDate = DateTime.UtcNow.AddSeconds(_tokenExpirationOptions.RefreshToken);
            }

            IdentityResult result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new AuthenticationFailedException("Refresh token could not be updated.");
        }

        private async Task ClearRefreshTokenAsync(AppUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            user.RefreshToken = null;
            user.RefreshTokenEndDate = null;

            IdentityResult result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new AuthenticationFailedException("Refresh token could not be cleared.");
        }

        private CookieOptions BuildAccessTokenCookieOptions(DateTime expiration)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = _authCookieOptions.Secure,
                Expires = expiration,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            };
        }

        private void DeleteAccessTokenCookie()
        {
            HttpContext httpContext = GetRequiredHttpContext();

            httpContext.Response.Cookies.Delete(
                AccessTokenCookieName,
                BuildAccessTokenCookieDeletionOptions());
        }

        private CookieOptions BuildAccessTokenCookieDeletionOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = _authCookieOptions.Secure,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            };
        }

        private HttpContext GetRequiredHttpContext()
        {
            return _httpContextAccessor.HttpContext
                ?? throw new AuthenticationFailedException("HTTP context is not available.");
        }

        private static DateTime GetAccessTokenExpiration(ClaimsPrincipal principal)
        {
            string? expirationClaim = principal.Claims
                .FirstOrDefault(claim => claim.Type == "exp")
                ?.Value;

            if (string.IsNullOrWhiteSpace(expirationClaim))
                return DateTime.MinValue;

            return long.TryParse(expirationClaim, out long expUnix)
                ? DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime
                : DateTime.MinValue;
        }

        private sealed class ExternalLoginInfo
        {
            public string? Email { get; init; }
            public string? FirstName { get; init; }
            public string? LastName { get; init; }
            public string? FullName { get; init; }
        }
        #endregion
    }
}
