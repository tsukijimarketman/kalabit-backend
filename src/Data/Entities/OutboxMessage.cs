using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Entities;

public sealed class OutboxMessage : Entity
{
    public Guid NotificationId { get; set; }
    public Notification Notification { get; set; } = null!;
    public DateTimeOffset NotificationCreatedAt { get; set; }
    public required string Topic { get; set; }
    public required string MessageKey { get; set; }
    public required string EnvelopeJson { get; set; }
    public required string HeadersJson { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
}
