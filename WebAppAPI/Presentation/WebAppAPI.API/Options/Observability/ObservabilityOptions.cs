namespace WebAppAPI.API.Options.Observability
{
    public sealed class ObservabilityOptions
    {
        public const string SectionName = "Observability";

        public string ApplicationName { get; set; } = "WebAppAPI";
        public LoggingOptions Logging { get; set; } = new();
    }
}
