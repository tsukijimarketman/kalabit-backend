namespace Data.Entities;

public sealed class NotificationIdentity : Entity
{
    public Guid ClientId { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? PayloadHash { get; set; }
    public DateTimeOffset? IdempotencyExpiresAt { get; set; }
    public Client Client { get; set; } = null!;
    public Notification Notification { get; set; } = null!;
}
