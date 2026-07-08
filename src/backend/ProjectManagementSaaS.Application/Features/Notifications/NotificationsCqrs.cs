using MediatR;
using ProjectManagementSaaS.Application.Abstractions.Common;
using ProjectManagementSaaS.Application.Abstractions.Identity;
using ProjectManagementSaaS.Application.Abstractions.Notifications;
using ProjectManagementSaaS.Application.Abstractions.Persistence;
using ProjectManagementSaaS.Application.Abstractions.Projects;
using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Common.Security;
using ProjectManagementSaaS.Application.Features.Notifications.Contracts;
using ProjectManagementSaaS.Domain.Notifications;
using ProjectManagementSaaS.Domain.Organizations;
using ProjectManagementSaaS.Domain.Projects;
using ProjectManagementSaaS.Domain.Work;

namespace ProjectManagementSaaS.Application.Features.Notifications;

public sealed record ListNotificationsQuery(
    PagedRequest Request,
    NotificationType? Type,
    NotificationPriority? Priority,
    bool? IsRead,
    Guid? ProjectId) : IRequest<PagedResult<NotificationDto>>;

public sealed record GetUnreadNotificationCountQuery : IRequest<int>;

public sealed record MarkNotificationReadCommand(Guid Id) : IRequest;

public sealed record MarkNotificationUnreadCommand(Guid Id) : IRequest;

public sealed record MarkAllNotificationsReadCommand : IRequest;

public sealed record DeleteNotificationCommand(Guid Id) : IRequest;

public sealed record ClearReadNotificationsCommand : IRequest;

public sealed record GetNotificationPreferencesQuery : IRequest<NotificationPreferencesDto>;

public sealed record UpdateNotificationPreferencesCommand(NotificationPreferencesRequest Request) : IRequest<NotificationPreferencesDto>;

internal sealed class ListNotificationsQueryHandler(
    IRepository<Notification> notificationRepository,
    IPaginationService paginationService,
    IProjectAccessService projectAccessService,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<ListNotificationsQuery, PagedResult<NotificationDto>>
{
    public async Task<PagedResult<NotificationDto>> Handle(ListNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = NotificationRules.GetCurrentUserId(currentUserService);
        var now = dateTimeProvider.UtcNow;

        var query = notificationRepository.Query()
            .Where(notification => notification.UserId == userId
                && (!notification.ExpiresAt.HasValue || notification.ExpiresAt > now));

        if (request.ProjectId.HasValue)
        {
            await projectAccessService.EnsureCanAccessProjectAsync(request.ProjectId.Value, cancellationToken);
            query = query.Where(notification => notification.ProjectId == request.ProjectId.Value);
        }

        if (request.Type.HasValue)
        {
            query = query.Where(notification => notification.NotificationType == request.Type.Value);
        }

        if (request.Priority.HasValue)
        {
            query = query.Where(notification => notification.Priority == request.Priority.Value);
        }

        if (request.IsRead.HasValue)
        {
            query = query.Where(notification => notification.IsRead == request.IsRead.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
        {
            var term = request.Request.Search.Trim();
            query = query.Where(notification =>
                notification.Title.Contains(term)
                || notification.Message.Contains(term)
                || (notification.Project != null && notification.Project.Name.Contains(term))
                || (notification.WorkItem != null && notification.WorkItem.Title.Contains(term)));
        }

        query = ApplySort(query, request.Request.SortBy, request.Request.SortDirection);

        return await paginationService.CreateAsync(
            query.Select(NotificationRules.ToDto()),
            request.Request.PageNumber,
            request.Request.PageSize,
            cancellationToken);
    }

    private static IQueryable<Notification> ApplySort(IQueryable<Notification> query, string? sortBy, string? direction)
    {
        var descending = direction?.Equals("asc", StringComparison.OrdinalIgnoreCase) != true;

        return (sortBy?.ToLowerInvariant(), descending) switch
        {
            ("priority", true) => query.OrderByDescending(notification => notification.Priority).ThenByDescending(notification => notification.CreatedAt),
            ("priority", false) => query.OrderBy(notification => notification.Priority).ThenByDescending(notification => notification.CreatedAt),
            ("type", true) => query.OrderByDescending(notification => notification.NotificationType).ThenByDescending(notification => notification.CreatedAt),
            ("type", false) => query.OrderBy(notification => notification.NotificationType).ThenByDescending(notification => notification.CreatedAt),
            ("createdat", false) => query.OrderBy(notification => notification.CreatedAt),
            _ => query.OrderByDescending(notification => notification.CreatedAt)
        };
    }
}

internal sealed class GetUnreadNotificationCountQueryHandler(
    IRepository<Notification> notificationRepository,
    IPaginationService paginationService,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    public async Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        var userId = NotificationRules.GetCurrentUserId(currentUserService);

        return await NotificationRules.CountUnreadAsync(notificationRepository, paginationService, userId, dateTimeProvider.UtcNow, cancellationToken);
    }
}

internal sealed class MarkNotificationReadCommandHandler(
    IRepository<Notification> notificationRepository,
    IPaginationService paginationService,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    INotificationRealtimePublisher realtimePublisher)
    : IRequestHandler<MarkNotificationReadCommand>
{
    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var userId = NotificationRules.GetCurrentUserId(currentUserService);
        var notification = await NotificationRules.GetOwnedNotificationAsync(notificationRepository, paginationService, request.Id, userId, cancellationToken);

        notification.IsRead = true;
        notification.ReadAt ??= dateTimeProvider.UtcNow;
        notificationRepository.Update(notification);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var unreadCount = await NotificationRules.CountUnreadAsync(notificationRepository, paginationService, userId, dateTimeProvider.UtcNow, cancellationToken);
        await realtimePublisher.PublishNotificationUpdatedAsync(userId, notification.Id, unreadCount, "read", cancellationToken);
    }
}

internal sealed class MarkNotificationUnreadCommandHandler(
    IRepository<Notification> notificationRepository,
    IPaginationService paginationService,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    INotificationRealtimePublisher realtimePublisher)
    : IRequestHandler<MarkNotificationUnreadCommand>
{
    public async Task Handle(MarkNotificationUnreadCommand request, CancellationToken cancellationToken)
    {
        var userId = NotificationRules.GetCurrentUserId(currentUserService);
        var notification = await NotificationRules.GetOwnedNotificationAsync(notificationRepository, paginationService, request.Id, userId, cancellationToken);

        notification.IsRead = false;
        notification.ReadAt = null;
        notificationRepository.Update(notification);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var unreadCount = await NotificationRules.CountUnreadAsync(notificationRepository, paginationService, userId, dateTimeProvider.UtcNow, cancellationToken);
        await realtimePublisher.PublishNotificationUpdatedAsync(userId, notification.Id, unreadCount, "unread", cancellationToken);
    }
}

internal sealed class MarkAllNotificationsReadCommandHandler(
    IRepository<Notification> notificationRepository,
    IPaginationService paginationService,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    INotificationRealtimePublisher realtimePublisher)
    : IRequestHandler<MarkAllNotificationsReadCommand>
{
    public async Task Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = NotificationRules.GetCurrentUserId(currentUserService);
        var now = dateTimeProvider.UtcNow;
        var unread = await paginationService.ToListAsync(
            notificationRepository.Query()
                .Where(notification => notification.UserId == userId
                    && !notification.IsRead
                    && (!notification.ExpiresAt.HasValue || notification.ExpiresAt > now)),
            cancellationToken);

        foreach (var notification in unread)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
            notificationRepository.Update(notification);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await realtimePublisher.PublishUnreadCountAsync(userId, 0, cancellationToken);
    }
}

internal sealed class DeleteNotificationCommandHandler(
    IRepository<Notification> notificationRepository,
    IPaginationService paginationService,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    INotificationRealtimePublisher realtimePublisher)
    : IRequestHandler<DeleteNotificationCommand>
{
    public async Task Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var userId = NotificationRules.GetCurrentUserId(currentUserService);
        var notification = await NotificationRules.GetOwnedNotificationAsync(notificationRepository, paginationService, request.Id, userId, cancellationToken);

        notificationRepository.Remove(notification);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var unreadCount = await NotificationRules.CountUnreadAsync(notificationRepository, paginationService, userId, dateTimeProvider.UtcNow, cancellationToken);
        await realtimePublisher.PublishNotificationUpdatedAsync(userId, notification.Id, unreadCount, "deleted", cancellationToken);
    }
}

internal sealed class ClearReadNotificationsCommandHandler(
    IRepository<Notification> notificationRepository,
    IPaginationService paginationService,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    INotificationRealtimePublisher realtimePublisher)
    : IRequestHandler<ClearReadNotificationsCommand>
{
    public async Task Handle(ClearReadNotificationsCommand request, CancellationToken cancellationToken)
    {
        var userId = NotificationRules.GetCurrentUserId(currentUserService);
        var read = await paginationService.ToListAsync(
            notificationRepository.Query()
                .Where(notification => notification.UserId == userId && notification.IsRead),
            cancellationToken);

        notificationRepository.RemoveRange(read);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var unreadCount = await NotificationRules.CountUnreadAsync(notificationRepository, paginationService, userId, dateTimeProvider.UtcNow, cancellationToken);
        await realtimePublisher.PublishNotificationUpdatedAsync(userId, Guid.Empty, unreadCount, "clear-read", cancellationToken);
    }
}

internal sealed class GetNotificationPreferencesQueryHandler(
    IRepository<NotificationPreference> preferenceRepository,
    IPaginationService paginationService,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetNotificationPreferencesQuery, NotificationPreferencesDto>
{
    public async Task<NotificationPreferencesDto> Handle(GetNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var userId = NotificationRules.GetCurrentUserId(currentUserService);
        var preference = await NotificationRules.GetOrCreatePreferencesAsync(
            preferenceRepository,
            paginationService,
            unitOfWork,
            userId,
            dateTimeProvider.UtcNow,
            currentUserService.UserName,
            cancellationToken);

        return NotificationRules.ToPreferencesDto(preference);
    }
}

internal sealed class UpdateNotificationPreferencesCommandHandler(
    IRepository<NotificationPreference> preferenceRepository,
    IPaginationService paginationService,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateNotificationPreferencesCommand, NotificationPreferencesDto>
{
    public async Task<NotificationPreferencesDto> Handle(UpdateNotificationPreferencesCommand request, CancellationToken cancellationToken)
    {
        var userId = NotificationRules.GetCurrentUserId(currentUserService);
        var preference = await NotificationRules.GetOrCreatePreferencesAsync(
            preferenceRepository,
            paginationService,
            unitOfWork,
            userId,
            dateTimeProvider.UtcNow,
            currentUserService.UserName,
            cancellationToken);

        preference.AssignmentNotifications = request.Request.AssignmentNotifications;
        preference.CommentNotifications = request.Request.CommentNotifications;
        preference.MentionNotifications = request.Request.MentionNotifications;
        preference.ReplyNotifications = request.Request.ReplyNotifications;
        preference.ProjectNotifications = request.Request.ProjectNotifications;
        preference.EmailNotifications = request.Request.EmailNotifications;
        preference.BrowserNotifications = request.Request.BrowserNotifications;
        preference.SoundNotifications = request.Request.SoundNotifications;
        preference.UpdatedAt = dateTimeProvider.UtcNow;
        preference.UpdatedBy = currentUserService.UserName;

        preferenceRepository.Update(preference);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return NotificationRules.ToPreferencesDto(preference);
    }
}

internal sealed class NotificationProcessor(
    IRepository<Notification> notificationRepository,
    IRepository<NotificationPreference> preferenceRepository,
    IRepository<NotificationDelivery> deliveryRepository,
    IRepository<ProjectMember> projectMemberRepository,
    IRepository<UserProfile> userProfileRepository,
    IPaginationService paginationService,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    IUserPermissionService userPermissionService,
    INotificationRealtimePublisher realtimePublisher)
    : INotificationProcessor
{
    public async Task ProcessAsync(NotificationIntent intent, CancellationToken cancellationToken)
    {
        var recipientIds = intent.UserIds
            .Where(userId => userId != Guid.Empty)
            .Distinct()
            .ToHashSet();

        if (intent.ExcludeActor && intent.ActorUserId.HasValue)
        {
            recipientIds.Remove(intent.ActorUserId.Value);
        }

        if (recipientIds.Count == 0)
        {
            return;
        }

        recipientIds = (await paginationService.ToListAsync(
            userProfileRepository.Query()
                .Where(profile => recipientIds.Contains(profile.UserId) && profile.IsActive)
                .Select(profile => profile.UserId),
            cancellationToken)).ToHashSet();

        if (intent.ProjectId.HasValue)
        {
            recipientIds = (await paginationService.ToListAsync(
                projectMemberRepository.Query()
                    .Where(member => member.ProjectId == intent.ProjectId.Value
                        && recipientIds.Contains(member.UserId)
                        && member.IsActive
                        && member.UserProfile != null
                        && member.UserProfile.IsActive)
                    .Select(member => member.UserId),
                cancellationToken)).ToHashSet();
        }

        if (recipientIds.Count == 0)
        {
            return;
        }

        var preferences = await paginationService.ToListAsync(
            preferenceRepository.Query().Where(preference => recipientIds.Contains(preference.UserId)),
            cancellationToken);
        var preferencesByUserId = preferences.ToDictionary(preference => preference.UserId);

        var now = dateTimeProvider.UtcNow;
        var createdNotifications = new List<Notification>();
        var createdDeliveries = new List<NotificationDelivery>();

        foreach (var recipientId in recipientIds)
        {
            var permissionCodes = await userPermissionService.GetPermissionCodesAsync(recipientId, cancellationToken);
            if (!permissionCodes.Contains(ApplicationPermissions.NotificationsView, StringComparer.Ordinal))
            {
                continue;
            }

            preferencesByUserId.TryGetValue(recipientId, out var preference);
            if (!NotificationRules.IsEnabled(intent.NotificationType, preference))
            {
                continue;
            }

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = recipientId,
                ProjectId = intent.ProjectId,
                WorkItemId = intent.WorkItemId,
                Title = intent.Title,
                Message = intent.Message,
                NotificationType = intent.NotificationType,
                Priority = intent.Priority,
                ActionUrl = intent.ActionUrl,
                Icon = intent.Icon,
                CreatedBy = intent.CreatedBy,
                CreatedAt = now
            };

            var delivery = new NotificationDelivery
            {
                Id = Guid.NewGuid(),
                NotificationId = notification.Id,
                UserId = recipientId,
                Channel = NotificationChannel.InApp,
                Status = NotificationDeliveryStatus.Pending,
                CreatedAt = now
            };

            await notificationRepository.AddAsync(notification, cancellationToken);
            await deliveryRepository.AddAsync(delivery, cancellationToken);
            createdNotifications.Add(notification);
            createdDeliveries.Add(delivery);
        }

        if (createdNotifications.Count == 0)
        {
            return;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var notification in createdNotifications)
        {
            var unreadCount = await NotificationRules.CountUnreadAsync(notificationRepository, paginationService, notification.UserId, now, cancellationToken);
            await realtimePublisher.PublishCreatedAsync(NotificationRules.ToRealtimePayload(notification, unreadCount), cancellationToken);
            await PublishRelatedRealtimeEventsAsync(notification, cancellationToken);
        }

        foreach (var delivery in createdDeliveries)
        {
            delivery.Status = NotificationDeliveryStatus.Delivered;
            delivery.DeliveredAt = dateTimeProvider.UtcNow;
            deliveryRepository.Update(delivery);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private Task PublishRelatedRealtimeEventsAsync(Notification notification, CancellationToken cancellationToken)
    {
        if (!notification.WorkItemId.HasValue)
        {
            return Task.CompletedTask;
        }

        return notification.NotificationType switch
        {
            NotificationType.Comment => realtimePublisher.PublishCommentChangedAsync(notification.UserId, notification.WorkItemId.Value, null, "comment.created", cancellationToken),
            NotificationType.Reply => realtimePublisher.PublishCommentChangedAsync(notification.UserId, notification.WorkItemId.Value, null, "reply.created", cancellationToken),
            NotificationType.Mention => realtimePublisher.PublishCommentChangedAsync(notification.UserId, notification.WorkItemId.Value, null, "mention.created", cancellationToken),
            NotificationType.TaskDeleted => realtimePublisher.PublishWorkItemChangedAsync(notification.UserId, notification.WorkItemId.Value, "workItem.deleted", cancellationToken),
            _ => realtimePublisher.PublishWorkItemChangedAsync(notification.UserId, notification.WorkItemId.Value, "workItem.updated", cancellationToken)
        };
    }
}

internal static class NotificationRules
{
    public static Guid GetCurrentUserId(ICurrentUserService currentUserService)
    {
        return currentUserService.UserId
            ?? throw new ApplicationUnauthorizedException("Authenticated user id claim is missing or invalid.");
    }

    public static System.Linq.Expressions.Expression<Func<Notification, NotificationDto>> ToDto()
    {
        return notification => new NotificationDto(
            notification.Id,
            notification.UserId,
            notification.ProjectId,
            notification.Project == null ? null : notification.Project.Name,
            notification.WorkItemId,
            notification.WorkItem == null ? null : notification.WorkItem.Title,
            notification.Title,
            notification.Message,
            notification.NotificationType,
            notification.Priority,
            notification.IsRead,
            notification.ReadAt,
            notification.ActionUrl,
            notification.Icon,
            notification.CreatedBy,
            notification.CreatedAt,
            notification.ExpiresAt);
    }

    public static async Task<Notification> GetOwnedNotificationAsync(
        IRepository<Notification> notificationRepository,
        IPaginationService paginationService,
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await paginationService.SingleOrDefaultAsync(
            notificationRepository.Query().Where(notification => notification.Id == notificationId && notification.UserId == userId),
            cancellationToken)
            ?? throw new ApplicationNotFoundException("Notification was not found.");
    }

    public static async Task<int> CountUnreadAsync(
        IRepository<Notification> notificationRepository,
        IPaginationService paginationService,
        Guid userId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var unreadIds = await paginationService.ToListAsync(
            notificationRepository.Query()
                .Where(notification => notification.UserId == userId
                    && !notification.IsRead
                    && (!notification.ExpiresAt.HasValue || notification.ExpiresAt > now))
                .Select(notification => notification.Id),
            cancellationToken);

        return unreadIds.Count;
    }

    public static bool IsEnabled(NotificationType type, NotificationPreference? preference)
    {
        return type switch
        {
            NotificationType.TaskAssigned
                or NotificationType.TaskReassigned
                or NotificationType.TaskCompleted
                or NotificationType.TaskReopened
                or NotificationType.TaskOverdue
                or NotificationType.TaskUpdated
                or NotificationType.TaskDeleted
                or NotificationType.StatusChanged
                or NotificationType.PriorityChanged
                or NotificationType.DueDateChanged
                or NotificationType.WatcherAdded => preference?.AssignmentNotifications ?? true,
            NotificationType.Comment or NotificationType.AttachmentAdded => preference?.CommentNotifications ?? true,
            NotificationType.Mention => preference?.MentionNotifications ?? true,
            NotificationType.Reply => preference?.ReplyNotifications ?? true,
            NotificationType.ProjectInvitation
                or NotificationType.ProjectRemoved
                or NotificationType.ProjectArchived
                or NotificationType.SprintStarted
                or NotificationType.SprintCompleted
                or NotificationType.ApprovalRequested
                or NotificationType.ApprovalCompleted => preference?.ProjectNotifications ?? true,
            NotificationType.SystemAnnouncement => true,
            _ => true
        };
    }

    public static async Task<NotificationPreference> GetOrCreatePreferencesAsync(
        IRepository<NotificationPreference> preferenceRepository,
        IPaginationService paginationService,
        IUnitOfWork unitOfWork,
        Guid userId,
        DateTimeOffset now,
        string? updatedBy,
        CancellationToken cancellationToken)
    {
        var preference = await paginationService.SingleOrDefaultAsync(
            preferenceRepository.Query().Where(entity => entity.UserId == userId),
            cancellationToken);

        if (preference is not null)
        {
            return preference;
        }

        preference = new NotificationPreference
        {
            UserId = userId,
            UpdatedAt = now,
            UpdatedBy = updatedBy
        };

        await preferenceRepository.AddAsync(preference, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return preference;
    }

    public static NotificationPreferencesDto ToPreferencesDto(NotificationPreference preference)
    {
        return new NotificationPreferencesDto(
            preference.UserId,
            preference.AssignmentNotifications,
            preference.CommentNotifications,
            preference.MentionNotifications,
            preference.ReplyNotifications,
            preference.ProjectNotifications,
            preference.EmailNotifications,
            preference.BrowserNotifications,
            preference.SoundNotifications,
            preference.UpdatedAt);
    }

    public static RealtimeNotificationPayload ToRealtimePayload(Notification notification, int unreadCount)
    {
        return new RealtimeNotificationPayload(
            notification.Id,
            notification.UserId,
            notification.ProjectId,
            notification.WorkItemId,
            notification.Title,
            notification.Message,
            notification.NotificationType,
            notification.Priority,
            notification.IsRead,
            notification.ReadAt,
            notification.ActionUrl,
            notification.Icon,
            notification.CreatedBy,
            notification.CreatedAt,
            notification.ExpiresAt,
            unreadCount);
    }
}
