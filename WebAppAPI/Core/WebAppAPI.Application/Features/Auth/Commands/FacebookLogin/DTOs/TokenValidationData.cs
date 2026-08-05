using System.Text.Json.Serialization;

namespace WebAppAPI.Application.Features.Auth.Commands.FacebookLogin.DTOs
{
    public sealed class TokenValidationData
    {
        [JsonPropertyName("is_valid")]
        public bool IsValid { get; init; }

        [JsonPropertyName("user_id")]
        public string UserId { get; init; } = string.Empty;
    }
}
