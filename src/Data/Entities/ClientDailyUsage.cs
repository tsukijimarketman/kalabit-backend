namespace Data.Entities;

public sealed class ClientDailyUsage
{
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public DateOnly UsageDate { get; set; }
    public int Count { get; set; }
}
