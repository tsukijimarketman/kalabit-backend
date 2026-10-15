using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Entities;

public sealed class NotificationRecipient : Entity
{
    public Guid NotificationId { get; set; }
    public Notification Notification { get; set; } = null!;
    public DateTimeOffset NotificationCreatedAt { get; set; }
    public required byte[] RecipientHash { get; set; }
    public required string MaskedAddress { get; set; }
    public required byte[]? EncryptedAddressEnvelope { get; set; }
}
