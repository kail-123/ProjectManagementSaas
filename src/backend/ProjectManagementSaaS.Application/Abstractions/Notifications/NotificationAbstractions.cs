using ProjectManagementSaaS.Domain.Notifications;

namespace ProjectManagementSaaS.Application.Abstractions.Notifications;

public sealed record NotificationIntent(
    IReadOnlyCollection<Guid> UserIds,
    Guid? ProjectId,
    Guid? WorkItemId,
    NotificationType NotificationType,
    NotificationPriority Priority,
    string Title,
    string Message,
    string? ActionUrl,
    string? Icon,
    Guid? ActorUserId,
    string? CreatedBy,
    bool ExcludeActor = true);

public sealed record RealtimeNotificationPayload(
    Guid Id,
    Guid UserId,
    Guid? ProjectId,
    Guid? WorkItemId,
    string Title,
    string Message,
    NotificationType NotificationType,
    NotificationPriority Priority,
    bool IsRead,
    DateTimeOffset? ReadAt,
    string? ActionUrl,
    string? Icon,
    string? CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpiresAt,
    int UnreadCount);

public interface INotificationDispatcher
{
    ValueTask EnqueueAsync(NotificationIntent intent, CancellationToken cancellationToken);
}

public interface INotificationProcessor
{
    Task ProcessAsync(NotificationIntent intent, CancellationToken cancellationToken);
}

public interface INotificationRealtimePublisher
{
    Task PublishCreatedAsync(RealtimeNotificationPayload payload, CancellationToken cancellationToken);

    Task PublishUnreadCountAsync(Guid userId, int unreadCount, CancellationToken cancellationToken);

    Task PublishNotificationUpdatedAsync(Guid userId, Guid notificationId, int unreadCount, string action, CancellationToken cancellationToken);

    Task PublishWorkItemChangedAsync(Guid userId, Guid workItemId, string eventName, CancellationToken cancellationToken);

    Task PublishCommentChangedAsync(Guid userId, Guid workItemId, Guid? commentId, string eventName, CancellationToken cancellationToken);
}
