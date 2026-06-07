namespace WebAppAPI.Application.Options.Mail
{
    public sealed class MailOptions
    {
        public const string SectionName = "Mail";

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int? Port { get; set; }
        public bool? EnableSsl { get; set; }
        public string Host { get; set; } = string.Empty;

        public bool IsConfigured =>
                        !string.IsNullOrWhiteSpace(Username) &&
                        !string.IsNullOrWhiteSpace(Password) &&
                        Port.HasValue &&
                        EnableSsl.HasValue &&
                        !string.IsNullOrWhiteSpace(Host);
    }
}
