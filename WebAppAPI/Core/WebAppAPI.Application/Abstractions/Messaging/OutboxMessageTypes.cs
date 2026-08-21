namespace WebAppAPI.Application.Abstractions.Messaging
{
    public static class OutboxMessageTypes
    {
        public const string OrderStatusUpdateMail = "OrderStatusUpdateMail";
        public const string PasswordResetMail = "PasswordResetMail";
    }
}
