using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.Contracts;

public sealed record SendNotificationAcceptedResponse(
    Guid NotificationId,
    string Status,
    Guid CorrelationId);
