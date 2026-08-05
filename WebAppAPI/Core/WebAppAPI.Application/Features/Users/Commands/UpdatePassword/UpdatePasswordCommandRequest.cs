using MediatR;

namespace WebAppAPI.Application.Features.Users.Commands.UpdatePassword
{
    public sealed class UpdatePasswordCommandRequest : IRequest<UpdatePasswordCommandResponse>
    {
        public required string UserId { get; init; }
        public required string ResetToken { get; init; }
        public required string Password { get; init; }
        public required string PasswordConfirm { get; init; }
    }
}
