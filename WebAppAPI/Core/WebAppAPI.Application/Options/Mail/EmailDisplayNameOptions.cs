namespace WebAppAPI.Application.Options.Mail
{
    public sealed class EmailDisplayNameOptions
    {
        public const string SectionName = "EmailDisplayNames";

        public string AppName { get; set; } = string.Empty;
        public string PasswordResetSubject { get; set; } = string.Empty;
        public string OrderStatusUpdateSubject { get; set; } = string.Empty;
    }
}
