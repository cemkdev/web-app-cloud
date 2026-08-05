using System.Text.Json.Serialization;

namespace WebAppAPI.Application.Features.Auth.Commands.FacebookLogin.DTOs
{
    public sealed class TokenValidationResponse
    {
        [JsonPropertyName("data")]
        public TokenValidationData? Data { get; init; }
    }
}
