using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Lib.Contracts;

public sealed record SendNotificationRequest(
    List<string> To,
    string TemplateId,
    JsonElement Data,
    DateTimeOffset? ExpiresAt);
