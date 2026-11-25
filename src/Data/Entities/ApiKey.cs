namespace Data.Entities;

public sealed class ApiKey : Entity
{
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public required string Name { get; set; }
    public required string Prefix { get; set; }
    public required byte[] KeyHash { get; set; }
    public string[] Scopes { get; set; } = [];
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
    
}
