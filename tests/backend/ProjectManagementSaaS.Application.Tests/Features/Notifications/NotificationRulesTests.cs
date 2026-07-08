using ProjectManagementSaaS.Application.Features.Notifications;
using ProjectManagementSaaS.Domain.Notifications;
using Xunit;

namespace ProjectManagementSaaS.Application.Tests.Features.Notifications;

public sealed class NotificationRulesTests
{
    [Theory]
    [InlineData(NotificationType.TaskAssigned)]
    [InlineData(NotificationType.TaskReassigned)]
    [InlineData(NotificationType.TaskUpdated)]
    [InlineData(NotificationType.StatusChanged)]
    [InlineData(NotificationType.PriorityChanged)]
    [InlineData(NotificationType.DueDateChanged)]
    [InlineData(NotificationType.WatcherAdded)]
    public void AssignmentPreferencesControlTaskNotifications(NotificationType notificationType)
    {
        var preference = new NotificationPreference { AssignmentNotifications = false };

        Assert.False(NotificationRules.IsEnabled(notificationType, preference));
    }

    [Theory]
    [InlineData(NotificationType.Comment)]
    [InlineData(NotificationType.AttachmentAdded)]
    public void CommentPreferencesControlCommentNotifications(NotificationType notificationType)
    {
        var preference = new NotificationPreference { CommentNotifications = false };

        Assert.False(NotificationRules.IsEnabled(notificationType, preference));
    }

    [Fact]
    public void MentionPreferencesControlMentionNotifications()
    {
        var preference = new NotificationPreference { MentionNotifications = false };

        Assert.False(NotificationRules.IsEnabled(NotificationType.Mention, preference));
    }

    [Fact]
    public void ReplyPreferencesControlReplyNotifications()
    {
        var preference = new NotificationPreference { ReplyNotifications = false };

        Assert.False(NotificationRules.IsEnabled(NotificationType.Reply, preference));
    }

    [Theory]
    [InlineData(NotificationType.ProjectInvitation)]
    [InlineData(NotificationType.ProjectRemoved)]
    [InlineData(NotificationType.ProjectArchived)]
    public void ProjectPreferencesControlProjectNotifications(NotificationType notificationType)
    {
        var preference = new NotificationPreference { ProjectNotifications = false };

        Assert.False(NotificationRules.IsEnabled(notificationType, preference));
    }

    [Fact]
    public void MissingPreferencesDefaultToEnabled()
    {
        Assert.True(NotificationRules.IsEnabled(NotificationType.TaskAssigned, preference: null));
    }

    [Fact]
    public void SystemAnnouncementsAreAlwaysEnabled()
    {
        var preference = new NotificationPreference
        {
            AssignmentNotifications = false,
            CommentNotifications = false,
            MentionNotifications = false,
            ReplyNotifications = false,
            ProjectNotifications = false
        };

        Assert.True(NotificationRules.IsEnabled(NotificationType.SystemAnnouncement, preference));
    }
}
