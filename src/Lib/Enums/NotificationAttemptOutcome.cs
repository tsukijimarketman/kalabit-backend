using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.Enums
{
    public enum NotificationAttemptOutcome
    {
        SmtpAccepted,
        TransientFailure,
        PermanentFailure
    }
}
