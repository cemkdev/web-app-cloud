using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppAPI.Application.Features.Auth.Commands.FacebookLogin;
using WebAppAPI.Application.Features.Auth.Commands.GoogleLogin;
using WebAppAPI.Application.Features.Auth.Commands.Login;
using WebAppAPI.Application.Features.Auth.Commands.Logout;
using WebAppAPI.Application.Features.Auth.Commands.PasswordReset;
using WebAppAPI.Application.Features.Auth.Commands.RefreshTokenLogin;
using WebAppAPI.Application.Features.Auth.Commands.VerifyResetToken;
using WebAppAPI.Application.Features.Auth.Queries.IdentityCheck;
using WebAppAPI.Domain.Constants;

namespace WebAppAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet("identity-check")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        public async Task<ActionResult<IdentityCheckQueryResponse>> IdentityCheck(CancellationToken cancellationToken)
        {
            IdentityCheckQueryResponse response = await _mediator.Send(new IdentityCheckQueryRequest(), cancellationToken);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginCommandResponse>> Login([FromBody] LoginCommandRequest request, CancellationToken cancellationToken)
        {
            LoginCommandResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("refresh-token-login")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        public async Task<ActionResult<RefreshTokenLoginCommandResponse>> RefreshTokenLogin(CancellationToken cancellationToken)
        {
            RefreshTokenLoginCommandResponse response = await _mediator.Send(
                new RefreshTokenLoginCommandRequest(),
                cancellationToken);

            return Ok(response);
        }

        [HttpPost("google-login")]
        public async Task<ActionResult<GoogleLoginCommandResponse>> GoogleLogin([FromBody] GoogleLoginCommandRequest request, CancellationToken cancellationToken)
        {
            GoogleLoginCommandResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("facebook-login")]
        public async Task<ActionResult<FacebookLoginCommandResponse>> FacebookLogin([FromBody] FacebookLoginCommandRequest request, CancellationToken cancellationToken)
        {
            FacebookLoginCommandResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("logout")]
        public async Task<ActionResult<LogoutCommandResponse>> Logout(CancellationToken cancellationToken)
        {
            LogoutCommandResponse response = await _mediator.Send(
                new LogoutCommandRequest(),
                cancellationToken);

            return Ok(response);
        }

        [HttpPost("password-reset")]
        public async Task<ActionResult<PasswordResetCommandResponse>> PasswordReset([FromBody] string email, CancellationToken cancellationToken)
        {
            PasswordResetCommandResponse response = await _mediator.Send(new PasswordResetCommandRequest
            {
                Email = email
            },
            cancellationToken);

            return Ok(response);
        }

        [HttpPost("verify-reset-token")]
        public async Task<ActionResult<VerifyResetTokenCommandResponse>> VerifyResetToken([FromBody] VerifyResetTokenCommandRequest request, CancellationToken cancellationToken)
        {
            VerifyResetTokenCommandResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }
    }
}
