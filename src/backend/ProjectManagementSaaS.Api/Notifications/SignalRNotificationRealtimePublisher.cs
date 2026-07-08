using Microsoft.AspNetCore.SignalR;
using ProjectManagementSaaS.Api.Hubs;
using ProjectManagementSaaS.Application.Abstractions.Notifications;

namespace ProjectManagementSaaS.Api.Notifications;

internal sealed class SignalRNotificationRealtimePublisher(IHubContext<NotificationHub> hubContext)
    : INotificationRealtimePublisher
{
    public async Task PublishCreatedAsync(RealtimeNotificationPayload payload, CancellationToken cancellationToken)
    {
        await hubContext.Clients
            .Group(NotificationHub.GetUserGroup(payload.UserId))
            .SendAsync("notification.created", payload, cancellationToken);

        await PublishUnreadCountAsync(payload.UserId, payload.UnreadCount, cancellationToken);
    }

    public Task PublishUnreadCountAsync(Guid userId, int unreadCount, CancellationToken cancellationToken)
    {
        return hubContext.Clients
            .Group(NotificationHub.GetUserGroup(userId))
            .SendAsync("notification.unreadCountChanged", new { unreadCount }, cancellationToken);
    }

    public Task PublishNotificationUpdatedAsync(Guid userId, Guid notificationId, int unreadCount, string action, CancellationToken cancellationToken)
    {
        return hubContext.Clients
            .Group(NotificationHub.GetUserGroup(userId))
            .SendAsync("notification.updated", new { notificationId, unreadCount, action }, cancellationToken);
    }

    public Task PublishWorkItemChangedAsync(Guid userId, Guid workItemId, string eventName, CancellationToken cancellationToken)
    {
        return hubContext.Clients
            .Group(NotificationHub.GetUserGroup(userId))
            .SendAsync(eventName, new { workItemId }, cancellationToken);
    }

    public Task PublishCommentChangedAsync(Guid userId, Guid workItemId, Guid? commentId, string eventName, CancellationToken cancellationToken)
    {
        return hubContext.Clients
            .Group(NotificationHub.GetUserGroup(userId))
            .SendAsync(eventName, new { workItemId, commentId }, cancellationToken);
    }
}
