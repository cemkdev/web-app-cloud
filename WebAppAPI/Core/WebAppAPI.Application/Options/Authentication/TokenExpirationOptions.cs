namespace WebAppAPI.Application.Options.Authentication
{
    public sealed class TokenExpirationOptions
    {
        public const string SectionName = "TokenExpirations";

        public int AccessToken { get; set; }
        public int RefreshToken { get; set; }
        public int RefreshBeforeTime { get; set; }
    }
}
