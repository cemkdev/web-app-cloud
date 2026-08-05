using System.Text.Json.Serialization;

namespace WebAppAPI.Application.Features.Auth.Commands.FacebookLogin.DTOs
{
    public sealed class UserInfoResponse
    {
        [JsonPropertyName("first_name")]
        public string? FirstName { get; init; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; init; }

        [JsonPropertyName("name")]
        public string? FullName { get; init; }

        [JsonPropertyName("email")]
        public string? Email { get; init; }
    }
}
