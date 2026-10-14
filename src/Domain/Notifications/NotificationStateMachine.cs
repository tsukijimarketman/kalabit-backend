using Lib.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Notifications;

public static class NotificationStateMachine
{
    public static bool CanTransition(
        NotificationStatus current,
        NotificationStatus next) =>
        current switch
        {
            NotificationStatus.Queued =>
                next is NotificationStatus.Published
                     or NotificationStatus.Suppressed
                     or NotificationStatus.Expired,
           
            NotificationStatus.Published =>
                next is NotificationStatus.Sending
                     or NotificationStatus.DeadLettered
                     or NotificationStatus.Suppressed
                     or NotificationStatus.Expired,

            NotificationStatus.Sending => 
                next is NotificationStatus.SmtpAccepted
                     or NotificationStatus.Retrying
                     or NotificationStatus.Failed
                     or NotificationStatus.DeadLettered
                     or NotificationStatus.Suppressed
                     or NotificationStatus.Expired,

            NotificationStatus.Retrying => 
                next is NotificationStatus.Sending
                     or NotificationStatus.Failed
                     or NotificationStatus.DeadLettered
                     or NotificationStatus.Suppressed
                     or NotificationStatus.Expired,

            NotificationStatus.SmtpAccepted =>
                next is NotificationStatus.Delivered
                     or NotificationStatus.Bounced,
            
            _ => false
        };
}
