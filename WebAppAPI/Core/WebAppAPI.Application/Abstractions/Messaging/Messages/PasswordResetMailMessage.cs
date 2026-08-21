namespace WebAppAPI.Application.Abstractions.Messaging.Messages
{
    public sealed class PasswordResetMailMessage
    {
        public required string Recipient { get; init; }
        public required string UserId { get; init; }
        public required string FirstName { get; init; }
        public required string ResetToken { get; init; }
    }
}
