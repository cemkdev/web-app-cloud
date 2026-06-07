namespace WebAppAPI.Application.Options.Authentication
{
    public sealed class TokenOptions
    {
        public const string SectionName = "Token";

        public string Audience { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string SecurityKey { get; set; } = string.Empty;
    }
}
