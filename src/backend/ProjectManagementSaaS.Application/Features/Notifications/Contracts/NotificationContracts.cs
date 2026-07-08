using ProjectManagementSaaS.Domain.Notifications;

namespace ProjectManagementSaaS.Application.Features.Notifications.Contracts;

public sealed record NotificationDto(
    Guid Id,
    Guid UserId,
    Guid? ProjectId,
    string? ProjectName,
    Guid? WorkItemId,
    string? WorkItemTitle,
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
    DateTimeOffset? ExpiresAt);

public sealed record NotificationPreferencesDto(
    Guid UserId,
    bool AssignmentNotifications,
    bool CommentNotifications,
    bool MentionNotifications,
    bool ReplyNotifications,
    bool ProjectNotifications,
    bool EmailNotifications,
    bool BrowserNotifications,
    bool SoundNotifications,
    DateTimeOffset UpdatedAt);

public sealed record NotificationPreferencesRequest(
    bool AssignmentNotifications,
    bool CommentNotifications,
    bool MentionNotifications,
    bool ReplyNotifications,
    bool ProjectNotifications,
    bool EmailNotifications,
    bool BrowserNotifications,
    bool SoundNotifications);
