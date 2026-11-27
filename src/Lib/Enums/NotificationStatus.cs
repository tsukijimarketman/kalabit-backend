namespace Lib.Enums;

public enum NotificationStatus
{
    Queued,
    /// <summary>
    /// Handed to Kafka and waiting for a consumer. Reserved for the Kafka phase (ADR-0002):
    /// in v0.1 the worker claims rows straight from Postgres, so notifications go
    /// <see cref="Queued"/> → <see cref="Sending"/> and never pass through this state.
    /// </summary>
    Published,
    Sending,
    Retrying,
    SmtpAccepted,
    Delivered,
    Bounced,
    Failed,
    DeadLettered,
    Suppressed,
    Expired
}
