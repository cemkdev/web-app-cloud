using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Exceptions;

namespace WebAppAPI.Application.Features.Users.Commands.UpdatePassword
{
    public class UpdatePasswordCommandHandler : IRequestHandler<UpdatePasswordCommandRequest, UpdatePasswordCommandResponse>
    {
        readonly IUserService _userService;

        public UpdatePasswordCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<UpdatePasswordCommandResponse> Handle(UpdatePasswordCommandRequest request, CancellationToken cancellationToken)
        {
            if (request.Password.Equals(request.PasswordConfirm))
                await _userService.UpdatePasswordAsync(request.UserId, request.ResetToken, request.Password);
            else
                throw new PasswordChangeFailedException("The confirm password does not match the password.");

            return new();
        }
    }
}
