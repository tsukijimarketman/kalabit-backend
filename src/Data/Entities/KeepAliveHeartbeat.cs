namespace Data.Entities;

public sealed class KeepAliveHeartbeat
{
    public long Id { get; set; }
    public DateTimeOffset PingedAt { get; set; }
    public required string Source { get; set; }
}
