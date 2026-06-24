using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebAppAPI.Application.Features.Auth.Commands.FacebookLogin;
using WebAppAPI.Application.Features.Auth.Commands.GoogleLogin;
using WebAppAPI.Application.Features.Auth.Commands.Login;
using WebAppAPI.Application.Features.Auth.Commands.Logout;
using WebAppAPI.Application.Features.Auth.Commands.PasswordReset;
using WebAppAPI.Application.Features.Auth.Commands.RefreshTokenLogin;
using WebAppAPI.Application.Features.Auth.Commands.VerifyResetToken;
using WebAppAPI.Application.Features.Auth.Queries.IdentityCheck;
using WebAppAPI.Application.Options.Mail;
using WebAppAPI.Domain.Constants;

namespace WebAppAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        readonly IMediator _mediator;
        private readonly MailOptions _mailOptions;

        public AuthController(IMediator mediator, IOptions<MailOptions> mailOptions)
        {
            _mediator = mediator;
            _mailOptions = mailOptions.Value;
        }

        [HttpGet("identity-check")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        public async Task<IActionResult> IdentityCheck([FromQuery] IdentityCheckQueryRequest identityCheckQueryRequest)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            IdentityCheckQueryResponse response = await _mediator.Send(identityCheckQueryRequest);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommandRequest loginCommandRequest)
        {
            LoginCommandResponse response = await _mediator.Send(loginCommandRequest);
            return Ok(response);
        }

        [HttpPost("refresh-token-login")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        public async Task<IActionResult> RefreshTokenLogin([FromBody] RefreshTokenLoginCommandRequest refreshTokenLoginCommandRequest)
        {
            RefreshTokenLoginCommandResponse response = await _mediator.Send(refreshTokenLoginCommandRequest);
            return Ok(response);
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin(GoogleLoginCommandRequest googleLoginCommandRequest)
        {
            GoogleLoginCommandResponse response = await _mediator.Send(googleLoginCommandRequest);
            return Ok(response);
        }

        [HttpPost("facebook-login")]
        public async Task<IActionResult> FacebookLogin(FacebookLoginCommandRequest facebookLoginCommandRequest)
        {
            FacebookLoginCommandResponse response = await _mediator.Send(facebookLoginCommandRequest);
            return Ok(response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogoutCommandRequest logoutCommandRequest)
        {
            LogoutCommandResponse response = await _mediator.Send(logoutCommandRequest);
            return Ok(response);
        }

        [HttpPost("password-reset")]
        public async Task<IActionResult> PasswordReset([FromBody] PasswordResetCommandRequest passwordResetCommandRequest)
        {
            if (!_mailOptions.IsConfigured)
                return Ok(new { Message = "Mail service is disabled because mail settings are not configured." });

            PasswordResetCommandResponse response = await _mediator.Send(passwordResetCommandRequest);
            return Ok(response);
        }

        [HttpPost("verify-reset-token")]
        public async Task<IActionResult> VerifyResetToken([FromBody] VerifyResetTokenCommandRequest verifyResetTokenCommandRequest)
        {
            VerifyResetTokenCommandResponse response = await _mediator.Send(verifyResetTokenCommandRequest);
            return Ok(response);
        }
    }
}
