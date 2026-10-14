using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.Enums;

public enum NotificationStatus
{
    Queued,
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
