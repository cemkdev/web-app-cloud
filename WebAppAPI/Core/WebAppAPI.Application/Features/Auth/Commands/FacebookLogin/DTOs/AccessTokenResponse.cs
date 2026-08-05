using System.Text.Json.Serialization;

namespace WebAppAPI.Application.Features.Auth.Commands.FacebookLogin.DTOs
{
    public sealed class AccessTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = string.Empty;
    }
}
