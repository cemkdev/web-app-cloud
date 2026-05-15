namespace WebAppAPI.API.Options.Observability
{
    public sealed class LoggingOptions
    {
        public string Provider { get; set; } = LoggingProviders.Seq;
        public string MinimumLevel { get; set; } = "Information";
        public SeqOptions Seq { get; set; } = new();
        public ElasticOptions Elastic { get; set; } = new();
        public ConsoleLoggingOptions Console { get; set; } = new();
    }
}
