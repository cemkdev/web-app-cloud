namespace WebAppAPI.Persistence.Outbox
{
    public enum OutboxMessageStatus
    {
        Pending = 1,
        Processing = 2,
        Processed = 3,
        Failed = 4,
        Expired = 5
    }
}
