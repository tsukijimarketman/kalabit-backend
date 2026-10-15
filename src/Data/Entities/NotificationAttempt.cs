using Lib.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Entities;

public sealed class NotificationAttempt : Entity
{
    public Guid NotificationId { get; set; }
    public Notification Notification { get; set; } = null!;
    public DateTimeOffset NotificationCreatedAt { get; set; }
    public int AttemptNumber { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    public NotificationAttemptOutcome Outcome { get; set; }
    public int? SmtpStatusCode { get; set; }
    public required string WorkerId { get; set; }
    public long DurationMilliseconds { get; set; }
}
