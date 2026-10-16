using Domain.Notifications;
using Lib.Enums;

namespace NotificationPlatform.UnitTests;

public class NotificationStateMachineTests
{
    [Fact]
    public void CanTransition_ReturnsExpectedResultForEveryStatusPair()
    {
        var allowedTransitions = new HashSet<(NotificationStatus Current, NotificationStatus Next)> 
        {
            (NotificationStatus.Queued, NotificationStatus.Published),
            (NotificationStatus.Queued, NotificationStatus.Suppressed),
            (NotificationStatus.Queued, NotificationStatus.Expired),

            (NotificationStatus.Published, NotificationStatus.Sending),
            (NotificationStatus.Published, NotificationStatus.DeadLettered),
            (NotificationStatus.Published, NotificationStatus.Suppressed),
            (NotificationStatus.Published, NotificationStatus.Expired),

            (NotificationStatus.Sending, NotificationStatus.SmtpAccepted),
            (NotificationStatus.Sending, NotificationStatus.Retrying),
            (NotificationStatus.Sending, NotificationStatus.Failed),
            (NotificationStatus.Sending, NotificationStatus.DeadLettered),
            (NotificationStatus.Sending, NotificationStatus.Suppressed),
            (NotificationStatus.Sending, NotificationStatus.Expired),

            (NotificationStatus.Retrying, NotificationStatus.Sending),
            (NotificationStatus.Retrying, NotificationStatus.Failed),
            (NotificationStatus.Retrying, NotificationStatus.DeadLettered),
            (NotificationStatus.Retrying, NotificationStatus.Suppressed),
            (NotificationStatus.Retrying, NotificationStatus.Expired),

            (NotificationStatus.SmtpAccepted, NotificationStatus.Delivered),
            (NotificationStatus.SmtpAccepted, NotificationStatus.Bounced),
        };

        foreach (var current in Enum.GetValues<NotificationStatus>())
        {
            foreach (var next in Enum.GetValues<NotificationStatus>())
            {
                var expected = allowedTransitions.Contains((current, next));
                var actual = NotificationStateMachine.CanTransition(current, next);

                Assert.True(
                    actual == expected,
                    $"Transition {current} --> {next} was {actual}; expected {expected}");
            }
        }
    }
}
