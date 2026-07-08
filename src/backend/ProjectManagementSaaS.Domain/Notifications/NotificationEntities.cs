using ProjectManagementSaaS.Domain.Organizations;
using ProjectManagementSaaS.Domain.Projects;
using ProjectManagementSaaS.Domain.Work;

namespace ProjectManagementSaaS.Domain.Notifications;

public sealed class Notification
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? ProjectId { get; set; }

    public Guid? WorkItemId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public NotificationType NotificationType { get; set; }

    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    public bool IsRead { get; set; }

    public DateTimeOffset? ReadAt { get; set; }

    public string? ActionUrl { get; set; }

    public string? Icon { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public UserProfile? User { get; set; }

    public Project? Project { get; set; }

    public WorkItem? WorkItem { get; set; }
}

public sealed class NotificationPreference
{
    public Guid UserId { get; set; }

    public bool AssignmentNotifications { get; set; } = true;

    public bool CommentNotifications { get; set; } = true;

    public bool MentionNotifications { get; set; } = true;

    public bool ReplyNotifications { get; set; } = true;

    public bool ProjectNotifications { get; set; } = true;

    public bool EmailNotifications { get; set; }

    public bool BrowserNotifications { get; set; }

    public bool SoundNotifications { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public UserProfile? User { get; set; }
}

public sealed class NotificationDelivery
{
    public Guid Id { get; set; }

    public Guid NotificationId { get; set; }

    public Guid UserId { get; set; }

    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;

    public NotificationDeliveryStatus Status { get; set; } = NotificationDeliveryStatus.Pending;

    public string? Error { get; set; }

    public DateTimeOffset? DeliveredAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Notification Notification { get; set; } = null!;
}

public sealed class NotificationOutbox
{
    public Guid Id { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public NotificationOutboxStatus Status { get; set; } = NotificationOutboxStatus.Pending;

    public int Attempts { get; set; }

    public string? LastError { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }
}

public enum NotificationType
{
    TaskAssigned,
    TaskReassigned,
    TaskCompleted,
    TaskReopened,
    TaskOverdue,
    Mention,
    Reply,
    Comment,
    StatusChanged,
    PriorityChanged,
    DueDateChanged,
    AttachmentAdded,
    ProjectInvitation,
    ProjectRemoved,
    SprintStarted,
    SprintCompleted,
    ApprovalRequested,
    ApprovalCompleted,
    ProjectArchived,
    SystemAnnouncement,
    TaskUpdated,
    TaskDeleted,
    WatcherAdded
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Critical
}

public enum NotificationChannel
{
    InApp
}

public enum NotificationDeliveryStatus
{
    Pending,
    Delivered,
    Skipped,
    Failed
}

public enum NotificationOutboxStatus
{
    Pending,
    Processed,
    Failed
}
