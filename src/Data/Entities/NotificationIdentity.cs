namespace Data.Entities;

public sealed class NotificationIdentity : Entity
{
    public Guid ClientId { get; set; }
    public required string IdempotencyKey { get; set; }
    public required string PayloadHash { get; set; }
    public DateTimeOffset IdempotencyExpiresAt { get; set; }
    public Client Client { get; set; } = null!;
}
