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
        public async Task<ActionResult<IdentityCheckQueryResponse>> IdentityCheck()
        {
            IdentityCheckQueryResponse response = await _mediator.Send(new IdentityCheckQueryRequest());
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginCommandResponse>> Login([FromBody] LoginCommandRequest request)
        {
            LoginCommandResponse response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpPost("refresh-token-login")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        public async Task<ActionResult<RefreshTokenLoginCommandResponse>> RefreshTokenLogin()
        {
            RefreshTokenLoginCommandResponse response = await _mediator.Send(new RefreshTokenLoginCommandRequest());
            return Ok(response);
        }

        [HttpPost("google-login")]
        public async Task<ActionResult<GoogleLoginCommandResponse>> GoogleLogin([FromBody] GoogleLoginCommandRequest request)
        {
            GoogleLoginCommandResponse response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpPost("facebook-login")]
        public async Task<ActionResult<FacebookLoginCommandResponse>> FacebookLogin([FromBody] FacebookLoginCommandRequest request)
        {
            FacebookLoginCommandResponse response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpPost("logout")]
        public async Task<ActionResult<LogoutCommandResponse>> Logout()
        {
            LogoutCommandResponse response = await _mediator.Send(new LogoutCommandRequest());
            return Ok(response);
        }

        [HttpPost("password-reset")]
        public async Task<ActionResult<PasswordResetCommandResponse>> PasswordReset([FromBody] string email)
        {
            if (!_mailOptions.IsConfigured)
                return Ok(new { Message = "Mail service is disabled because mail settings are not configured." });

            PasswordResetCommandResponse response = await _mediator.Send(new PasswordResetCommandRequest
            {
                Email = email
            });

            return Ok(response);
        }

        [HttpPost("verify-reset-token")]
        public async Task<ActionResult<VerifyResetTokenCommandResponse>> VerifyResetToken([FromBody] VerifyResetTokenCommandRequest request)
        {
            VerifyResetTokenCommandResponse response = await _mediator.Send(request);
            return Ok(response);
        }
    }
}
