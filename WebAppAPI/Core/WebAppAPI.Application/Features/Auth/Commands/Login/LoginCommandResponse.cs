using WebAppAPI.Application.DTOs;

namespace WebAppAPI.Application.Features.Auth.Commands.Login
{
    public class LoginCommandResponse
    {
    }

    public class LoginSuccessCommandResponse : LoginCommandResponse
    {
        public Token Token { get; set; }
    }

    public class LoginErrorCommandResponse : LoginCommandResponse
    {
        public string Message { get; set; }
    }
}
