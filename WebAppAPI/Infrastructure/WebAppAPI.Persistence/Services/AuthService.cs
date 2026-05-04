using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text.Json;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Abstractions.Token;
using WebAppAPI.Application.DTOs;
using WebAppAPI.Application.DTOs.Facebook;
using WebAppAPI.Application.Exceptions;
using WebAppAPI.Application.Helpers;
using U = WebAppAPI.Domain.Entities.Identity;

namespace WebAppAPI.Persistence.Services
{
    public class AuthService : IAuthService
    {
        readonly HttpClient _httpClient;
        readonly IConfiguration _configuration;
        readonly UserManager<U.AppUser> _userManager;
        readonly ITokenHandler _tokenHandler;
        readonly SignInManager<U.AppUser> _signInManager;
        readonly IUserService _userService;
        readonly IMailService _mailService;
        IHttpContextAccessor _httpContextAccessor;
        readonly IRoleService _roleService;
        readonly bool _authCookieSecure;

        private readonly int refreshTokenExpirationTime;

        public AuthService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            UserManager<U.AppUser> userManager,
            ITokenHandler tokenHandler,
            SignInManager<U.AppUser> signInManager,
            IUserService userService,
            IMailService mailService,
            IHttpContextAccessor httpContextAccessor,
            IRoleService roleService)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _userManager = userManager;
            _tokenHandler = tokenHandler;
            _signInManager = signInManager;
            _userService = userService;
            _mailService = mailService;
            _httpContextAccessor = httpContextAccessor;

            refreshTokenExpirationTime = Convert.ToInt32(_configuration["TokenExpirations:RefreshToken"]);
            _roleService = roleService;

            _authCookieSecure = _configuration.GetValue<bool>("AuthCookie:Secure");
        }

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
            if (result.Succeeded) // Authentication succeeded!
            {
                await _userManager.ResetAccessFailedCountAsync(user);

                Token token = _tokenHandler.CreateAccessToken(user);
                string refreshToken = _tokenHandler.CreateRefreshToken();
                await _userService.UpdateRefreshTokenAsync(user, refreshToken, refreshTokenExpirationTime);

                // We're sending the access token as an HttpOnly cookie.
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
            // if "ExternalLoginSettings:Facebook" property is empty in appsettings.
            if (string.IsNullOrWhiteSpace(_configuration["ExternalLoginSettings:Facebook:Client_ID"]) ||
                string.IsNullOrWhiteSpace(_configuration["ExternalLoginSettings:Facebook:Client_Secret"]))
            {
                return null!;
            }

            string accessTokenResponse = await _httpClient.GetStringAsync($"https://graph.facebook.com/oauth/access_token?client_id={_configuration["ExternalLoginSettings:Facebook:Client_ID"]}&client_secret={_configuration["ExternalLoginSettings:Facebook:Client_Secret"]}&grant_type=client_credentials");

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
            // if "ExternalLoginSettings:Google" property is empty in appsettings.
            if (string.IsNullOrWhiteSpace(_configuration["ExternalLoginSettings:Google:Client_ID"]))
            {
                return null!;
            }

            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { _configuration["ExternalLoginSettings:Google:Client_ID"] }
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
            var isAdmin = await _userService.HasAdminAccessAsync(username);

            var expirationClaim = user.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            DateTime expirationDate = DateTime.MinValue;

            if (expirationClaim != null && long.TryParse(expirationClaim, out var expUnix))
            {
                // Convert Unix timestamp to DateTime.
                expirationDate = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            }

            string refreshBeforeTime = _configuration["TokenExpirations:RefreshBeforeTime"];

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

            // Even if the token has expired, let's extract the username.
            var username = _tokenHandler.GetUsernameFromExpiredToken(accessToken);
            if (string.IsNullOrEmpty(username))
                throw new AuthenticationFailedException("User info could not be extracted from token.");

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
                throw new NotFoundUserException("User not found during logout.");

            _httpContextAccessor.HttpContext.Response.Cookies.Delete("accessToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = _authCookieSecure,
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

                await _userManager.AddLoginAsync(user, info); //AspNetUserLogins                    

                Token token = _tokenHandler.CreateAccessToken(user);
                string refreshToken = _tokenHandler.CreateRefreshToken();
                await _userService.UpdateRefreshTokenAsync(user, refreshToken, refreshTokenExpirationTime);

                // We're sending the access token as an HttpOnly cookie.
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
                await _userService.UpdateRefreshTokenAsync(user, newRefreshToken, refreshTokenExpirationTime, true);
                SetHttpOnlyAccessTokenCookie(token);

                return token;
            }
            else
                throw new AuthenticationFailedException();
        }

        // Sending the token as an HttpOnly cookie.
        private void SetHttpOnlyAccessTokenCookie(Token token)
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Append("accessToken", token.AccessToken, new CookieOptions
            {
                HttpOnly = true, // Prevents JavaScript access.
                Secure = _authCookieSecure, // If true, the cookie will only be sent over HTTPS.
                Expires = token.Expiration, // Token's expiration time.
                SameSite = SameSiteMode.Strict, // To prevent CSRF.
                Path = "/" // It's only valid for the relevant path.
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
