using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Exceptions;
using WebAppAPI.Application.Features.Users.Commands.UpdatePassword.DTOs;

namespace WebAppAPI.Application.Features.Users.Commands.UpdatePassword
{
    public sealed class UpdatePasswordCommandHandler(IUserService userService) : IRequestHandler<UpdatePasswordCommandRequest, UpdatePasswordCommandResponse>
    {
        public async Task<UpdatePasswordCommandResponse> Handle(UpdatePasswordCommandRequest request, CancellationToken cancellationToken)
        {
            if (!string.Equals(
                request.Password,
                request.PasswordConfirm,
                StringComparison.Ordinal))
                throw new PasswordChangeFailedException("The confirm password does not match the password.");

            await userService.UpdatePasswordAsync(new ResetPasswordDto
            {
                UserId = request.UserId,
                ResetToken = request.ResetToken,
                NewPassword = request.Password
            });

            return new UpdatePasswordCommandResponse();
        }
    }
}
