namespace WebAppAPI.Application.Options.Authentication
{
    public sealed class AuthCookieOptions
    {
        public const string SectionName = "AuthCookie";

        public bool Secure { get; set; } = true;
    }
}
