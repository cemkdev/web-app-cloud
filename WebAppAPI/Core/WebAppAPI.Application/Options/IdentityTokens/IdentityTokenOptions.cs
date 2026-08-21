namespace WebAppAPI.Application.Options.IdentityTokens
{
    public sealed class IdentityTokenOptions
    {
        public const string SectionName = "IdentityTokens";

        public int LifetimeMinutes { get; set; } = 30;
    }
}
