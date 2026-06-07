namespace WebAppAPI.Application.Options.Authentication
{
    public sealed class ExternalLoginOptions
    {
        public const string SectionName = "ExternalLogin";

        public FacebookLoginOptions Facebook { get; set; } = new();
        public GoogleLoginOptions Google { get; set; } = new();
    }

    public sealed class FacebookLoginOptions
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }

    public sealed class GoogleLoginOptions
    {
        public string ClientId { get; set; } = string.Empty;
    }
}
