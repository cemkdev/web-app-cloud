namespace WebAppAPI.API.Options.Hosting
{
    public sealed class HostingOptions
    {
        public const string SectionName = "Hosting";

        public bool UseHttpsRedirection { get; set; } = true;
    }
}
