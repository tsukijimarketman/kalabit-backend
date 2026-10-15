using Lib.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Entities;

public sealed class Notification : Entity
{
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public Guid TemplateVersionId { get; set; }
    public Guid CorrelationId { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Queued;
    public DateTimeOffset? ExpiresAt { get; set; }
    public required byte[]? EncryptedPayloadEnvelope { get; set; }
    public TemplateVersion TemplateVersion { get; set; } = null!;
    public NotificationIdentity Identity { get; set; } = null!;
    public ICollection<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();
    public ICollection<NotificationAttempt> Attempts { get; set; } = new List<NotificationAttempt>();
}
