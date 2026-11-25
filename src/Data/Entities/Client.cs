using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Entities;

public sealed class Client : AuditableEntity, IDeactivatable
{
    public required string Name { get; set; }
    public string? KeycloakClientId { get; set; }
    public bool IsActive { get; set; } = true;
    public int RequestsPerMinute { get; set; }
    public int DailyQuota { get; set; }
    public bool IsSynthetic { get; set; }
    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
}
