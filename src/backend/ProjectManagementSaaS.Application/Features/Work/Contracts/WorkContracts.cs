using ProjectManagementSaaS.Domain.Work;

namespace ProjectManagementSaaS.Application.Features.Work.Contracts;

public sealed record WorkItemTypeDto(
    Guid Id,
    string Name,
    string Code,
    string Icon,
    string Color,
    Guid WorkflowId,
    string? Description,
    bool IsActive);

public sealed record WorkWorkflowDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsDefault,
    bool IsActive,
    IReadOnlyCollection<WorkWorkflowStatusDto> Statuses,
    IReadOnlyCollection<WorkWorkflowTransitionDto> Transitions);

public sealed record WorkWorkflowStatusDto(
    Guid Id,
    Guid WorkflowId,
    string Name,
    string Code,
    string Color,
    int SortOrder,
    bool IsInitial,
    bool IsTerminal,
    bool IsActive);

public sealed record WorkWorkflowTransitionDto(
    Guid Id,
    Guid WorkflowId,
    Guid FromStatusId,
    Guid ToStatusId,
    string Name,
    bool IsActive);

public sealed record WorkPriorityDto(
    Guid Id,
    string Name,
    string Code,
    string Color,
    int SortOrder,
    bool IsActive);

public sealed record WorkLabelDto(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string Color,
    string? Description,
    bool IsActive);

public sealed record WorkLabelUpsertRequest(
    Guid OrganizationId,
    string Name,
    string Color,
    string? Description,
    bool IsActive);

public sealed record WorkComponentDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    string? Description,
    string Color,
    bool IsActive);

public sealed record WorkWatcherDto(
    Guid UserProfileId,
    string DisplayName,
    DateTimeOffset AddedOn,
    string? AddedBy);

public sealed record WorkComponentUpsertRequest(
    Guid ProjectId,
    string Name,
    string? Description,
    string Color,
    bool IsActive);

public sealed record WorkMetadataDto(
    IReadOnlyCollection<WorkItemTypeDto> Types,
    IReadOnlyCollection<WorkWorkflowDto> Workflows,
    IReadOnlyCollection<WorkPriorityDto> Priorities,
    IReadOnlyCollection<WorkLabelDto> Labels,
    IReadOnlyCollection<WorkComponentDto> Components);

public sealed record WorkItemDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    Guid? ParentWorkItemId,
    string? ParentTitle,
    Guid WorkItemTypeId,
    string TypeName,
    string TypeIcon,
    string TypeColor,
    Guid WorkflowStatusId,
    string StatusName,
    string StatusColor,
    Guid PriorityId,
    string PriorityName,
    string PriorityColor,
    string Title,
    string? Description,
    string? AcceptanceCriteria,
    Guid? AssigneeUserProfileId,
    string? AssigneeName,
    Guid ReporterUserProfileId,
    string ReporterName,
    DateOnly? StartDate,
    DateOnly? DueDate,
    decimal? EstimatedHours,
    decimal LoggedHours,
    int? StoryPoints,
    Guid? SprintId,
    IReadOnlyCollection<WorkLabelDto> Labels,
    IReadOnlyCollection<WorkComponentDto> Components,
    string? Environment,
    string? Severity,
    bool? Reproducible,
    int CommentCount,
    int AttachmentCount,
    IReadOnlyCollection<WorkWatcherDto> Watchers,
    int WatcherCount,
    string RowVersion,
    string? CreatedBy,
    DateTimeOffset CreatedOn,
    string? UpdatedBy,
    DateTimeOffset? UpdatedOn);

public sealed record WorkItemUpsertRequest(
    Guid ProjectId,
    Guid? ParentWorkItemId,
    Guid WorkItemTypeId,
    Guid? WorkflowStatusId,
    Guid PriorityId,
    string Title,
    string? Description,
    string? AcceptanceCriteria,
    Guid? AssigneeUserProfileId,
    Guid ReporterUserProfileId,
    DateOnly? StartDate,
    DateOnly? DueDate,
    decimal? EstimatedHours,
    decimal LoggedHours,
    int? StoryPoints,
    Guid? SprintId,
    IReadOnlyCollection<Guid> LabelIds,
    IReadOnlyCollection<Guid> ComponentIds,
    IReadOnlyCollection<Guid> WatcherUserProfileIds,
    string? Environment,
    string? Severity,
    bool? Reproducible,
    string? RowVersion);

public sealed record WorkTransitionRequest(Guid ToStatusId, string RowVersion);

public sealed record WorkCommentDto(
    Guid Id,
    Guid WorkItemId,
    Guid? ParentCommentId,
    Guid? AuthorId,
    string AuthorName,
    string? AuthorRole,
    string Message,
    string BodyMarkdown,
    bool IsEdited,
    DateTimeOffset? EditedAt,
    bool IsDeleted,
    DateTimeOffset? DeletedAt,
    bool Pinned,
    bool Resolved,
    int ReplyCount,
    IReadOnlyCollection<Guid> MentionUserProfileIds,
    IReadOnlyCollection<Guid> MentionedUserIds,
    IReadOnlyCollection<WorkCommentMentionDto> Mentions,
    IReadOnlyCollection<WorkCommentAttachmentDto> Attachments,
    IReadOnlyCollection<WorkCommentReactionDto> Reactions,
    IReadOnlyCollection<WorkCommentReadDto> ReadReceipts,
    WorkCommentCountersDto Counters,
    bool ReadByCurrentUser,
    string? CreatedBy,
    DateTimeOffset CreatedOn,
    string? UpdatedBy,
    DateTimeOffset? UpdatedOn);

public sealed class WorkCommentUpsertRequest
{
    public Guid? ParentCommentId { get; init; }

    public string? Message { get; init; }

    public string? BodyMarkdown { get; init; }

    public IReadOnlyCollection<Guid> MentionUserProfileIds { get; init; } = Array.Empty<Guid>();

    public IReadOnlyCollection<Guid> MentionedUserIds { get; init; } = Array.Empty<Guid>();

    public string RequiredMessage => string.IsNullOrWhiteSpace(Message) ? BodyMarkdown ?? string.Empty : Message;
}

public sealed record WorkCommentMentionDto(
    Guid Id,
    Guid CommentId,
    Guid MentionedUserId,
    Guid UserProfileId,
    string DisplayName,
    string EmployeeCode,
    string? Designation,
    string? Department,
    string? ProfilePhoto,
    Guid? MentionedByUserId,
    DateTimeOffset CreatedAt);

public sealed record WorkMentionCandidateDto(
    Guid UserId,
    Guid UserProfileId,
    string DisplayName,
    string EmployeeCode,
    string Email,
    string? Designation,
    string? Department,
    string? ProfilePhoto);

public sealed record WorkCommentMentionsRequest(IReadOnlyCollection<Guid> MentionedUserIds);

public sealed record WorkCommentReactionDto(
    Guid Id,
    Guid CommentId,
    Guid UserId,
    string DisplayName,
    string Emoji,
    DateTimeOffset CreatedAt);

public sealed record WorkCommentReactionRequest(string Emoji);

public sealed record WorkCommentReadDto(
    Guid Id,
    Guid CommentId,
    Guid UserId,
    string DisplayName,
    DateTimeOffset ReadAt);

public sealed record WorkCommentCountersDto(
    int Replies,
    int Mentions,
    int Reactions,
    int Attachments,
    int SeenCount);

public sealed record WorkCommentHistoryDto(
    Guid Id,
    Guid CommentId,
    Guid? EditedBy,
    string EditedByName,
    DateTimeOffset EditedAt,
    string PreviousMessage,
    string NewMessage);

public sealed record WorkCommentAttachmentDto(
    Guid Id,
    Guid CommentId,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string StoragePath,
    string PreviewType,
    string? UploadedBy,
    DateTimeOffset UploadedAt);

public sealed record WorkCommentAttachmentRequest(
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string StoragePath);

public sealed record WorkAttachmentDto(
    Guid Id,
    Guid WorkItemId,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string StoragePath,
    int Version,
    string? Description,
    string? CreatedBy,
    DateTimeOffset CreatedOn);

public sealed record WorkAttachmentRequest(
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string StoragePath,
    int Version,
    string? Description);

public sealed record WorkActivityDto(
    Guid Id,
    Guid ProjectId,
    Guid? WorkItemId,
    Guid? UserId,
    string ActorName,
    string? ActorRole,
    string? ActorAvatar,
    WorkActivityType ActivityType,
    WorkActivityCategory Category,
    string Description,
    string? FieldName,
    string? OldValue,
    string? NewValue,
    DateTimeOffset CreatedAt);

public sealed record WorkItemLinkDto(
    Guid Id,
    Guid SourceWorkItemId,
    Guid TargetWorkItemId,
    string TargetTitle,
    WorkItemLinkType LinkType,
    string? Description);

public sealed record WorkItemLinkRequest(
    Guid TargetWorkItemId,
    WorkItemLinkType LinkType,
    string? Description);

public sealed record WorkSavedFilterDto(
    Guid Id,
    Guid? ProjectId,
    Guid? OwnerUserId,
    string Name,
    string QueryJson,
    bool IsShared,
    string? CreatedBy,
    DateTimeOffset CreatedOn);

public sealed record WorkSavedFilterRequest(
    Guid? ProjectId,
    string Name,
    string QueryJson,
    bool IsShared);
