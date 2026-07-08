using ProjectManagementSaaS.Domain.Common;
using ProjectManagementSaaS.Domain.Organizations;
using ProjectManagementSaaS.Domain.Projects;

namespace ProjectManagementSaaS.Domain.Work;

public sealed class WorkItemType : SoftDeletableAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public Guid WorkflowId { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public WorkWorkflow Workflow { get; set; } = null!;
}

public sealed class WorkWorkflow : SoftDeletableAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<WorkWorkflowStatus> Statuses { get; } = new List<WorkWorkflowStatus>();

    public ICollection<WorkWorkflowTransition> Transitions { get; } = new List<WorkWorkflowTransition>();
}

public sealed class WorkWorkflowStatus : SoftDeletableAuditableEntity
{
    public Guid WorkflowId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsInitial { get; set; }

    public bool IsTerminal { get; set; }

    public bool IsActive { get; set; } = true;

    public WorkWorkflow Workflow { get; set; } = null!;
}

public sealed class WorkWorkflowTransition : SoftDeletableAuditableEntity
{
    public Guid WorkflowId { get; set; }

    public Guid FromStatusId { get; set; }

    public Guid ToStatusId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public WorkWorkflow Workflow { get; set; } = null!;

    public WorkWorkflowStatus FromStatus { get; set; } = null!;

    public WorkWorkflowStatus ToStatus { get; set; } = null!;
}

public sealed class WorkPriority : SoftDeletableAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class WorkLabel : SoftDeletableAuditableEntity
{
    public Guid OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public Organization Organization { get; set; } = null!;
}

public sealed class WorkComponent : SoftDeletableAuditableEntity
{
    public Guid ProjectId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Color { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public Project Project { get; set; } = null!;
}

public sealed class WorkItem : SoftDeletableAuditableEntity
{
    public Guid ProjectId { get; set; }

    public Guid? ParentWorkItemId { get; set; }

    public Guid WorkItemTypeId { get; set; }

    public Guid WorkflowStatusId { get; set; }

    public Guid PriorityId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? AcceptanceCriteria { get; set; }

    public Guid? AssigneeUserProfileId { get; set; }

    public Guid ReporterUserProfileId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public decimal? EstimatedHours { get; set; }

    public decimal LoggedHours { get; set; }

    public int? StoryPoints { get; set; }

    public Guid? SprintId { get; set; }

    public string? Environment { get; set; }

    public string? Severity { get; set; }

    public bool? Reproducible { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public Project Project { get; set; } = null!;

    public WorkItem? ParentWorkItem { get; set; }

    public WorkItemType Type { get; set; } = null!;

    public WorkWorkflowStatus Status { get; set; } = null!;

    public WorkPriority Priority { get; set; } = null!;

    public UserProfile? Assignee { get; set; }

    public UserProfile Reporter { get; set; } = null!;

    public ICollection<WorkItem> Children { get; } = new List<WorkItem>();

    public ICollection<WorkItemLabel> Labels { get; } = new List<WorkItemLabel>();

    public ICollection<WorkItemComponent> Components { get; } = new List<WorkItemComponent>();

    public ICollection<WorkItemWatcher> Watchers { get; } = new List<WorkItemWatcher>();

    public ICollection<WorkItemLink> OutboundLinks { get; } = new List<WorkItemLink>();

    public ICollection<WorkItemLink> InboundLinks { get; } = new List<WorkItemLink>();

    public ICollection<WorkComment> Comments { get; } = new List<WorkComment>();

    public ICollection<WorkAttachment> Attachments { get; } = new List<WorkAttachment>();
}

public sealed class WorkItemLabel
{
    public Guid WorkItemId { get; set; }

    public Guid LabelId { get; set; }

    public WorkItem WorkItem { get; set; } = null!;

    public WorkLabel Label { get; set; } = null!;
}

public sealed class WorkItemComponent
{
    public Guid WorkItemId { get; set; }

    public Guid ComponentId { get; set; }

    public WorkItem WorkItem { get; set; } = null!;

    public WorkComponent Component { get; set; } = null!;
}

public sealed class WorkItemWatcher
{
    public Guid WorkItemId { get; set; }

    public Guid UserProfileId { get; set; }

    public DateTimeOffset AddedOn { get; set; }

    public string? AddedBy { get; set; }

    public WorkItem WorkItem { get; set; } = null!;

    public UserProfile UserProfile { get; set; } = null!;
}

public sealed class WorkItemLink : SoftDeletableAuditableEntity
{
    public Guid SourceWorkItemId { get; set; }

    public Guid TargetWorkItemId { get; set; }

    public WorkItemLinkType LinkType { get; set; }

    public string? Description { get; set; }

    public WorkItem SourceWorkItem { get; set; } = null!;

    public WorkItem TargetWorkItem { get; set; } = null!;
}

public sealed class WorkComment : SoftDeletableAuditableEntity
{
    public Guid WorkItemId { get; set; }

    public Guid? ParentCommentId { get; set; }

    public Guid? AuthorId { get; set; }

    public string Message { get; set; } = string.Empty;

    public string BodyMarkdown
    {
        get => Message;
        set => Message = value;
    }

    public bool IsEdited { get; set; }

    public DateTimeOffset? EditedAt { get; set; }

    public bool Pinned { get; set; }

    public bool Resolved { get; set; }

    public WorkItem WorkItem { get; set; } = null!;

    public UserProfile? Author { get; set; }

    public WorkComment? ParentComment { get; set; }

    public ICollection<WorkComment> Replies { get; } = new List<WorkComment>();

    public ICollection<WorkCommentMention> Mentions { get; } = new List<WorkCommentMention>();

    public ICollection<WorkCommentHistory> History { get; } = new List<WorkCommentHistory>();

    public ICollection<WorkCommentAttachment> Attachments { get; } = new List<WorkCommentAttachment>();

    public ICollection<WorkCommentReaction> Reactions { get; } = new List<WorkCommentReaction>();

    public ICollection<WorkCommentRead> Reads { get; } = new List<WorkCommentRead>();
}

public sealed class WorkCommentMention
{
    public Guid Id { get; set; }

    public Guid CommentId { get; set; }

    public Guid MentionedUserId { get; set; }

    public Guid? MentionedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public WorkComment Comment { get; set; } = null!;

    public UserProfile MentionedUser { get; set; } = null!;

    public UserProfile? MentionedByUser { get; set; }
}

public sealed class WorkCommentReaction
{
    public Guid Id { get; set; }

    public Guid CommentId { get; set; }

    public Guid UserId { get; set; }

    public string Emoji { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public WorkComment Comment { get; set; } = null!;

    public UserProfile User { get; set; } = null!;
}

public sealed class WorkCommentRead
{
    public Guid Id { get; set; }

    public Guid CommentId { get; set; }

    public Guid UserId { get; set; }

    public DateTimeOffset ReadAt { get; set; }

    public WorkComment Comment { get; set; } = null!;

    public UserProfile User { get; set; } = null!;
}

public sealed class WorkCommentHistory
{
    public Guid Id { get; set; }

    public Guid CommentId { get; set; }

    public Guid? EditedBy { get; set; }

    public string? EditedByName { get; set; }

    public DateTimeOffset EditedAt { get; set; }

    public string PreviousMessage { get; set; } = string.Empty;

    public string NewMessage { get; set; } = string.Empty;

    public WorkComment Comment { get; set; } = null!;

    public UserProfile? EditedByProfile { get; set; }
}

public sealed class WorkCommentAttachment : SoftDeletableAuditableEntity
{
    public Guid CommentId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public string StoragePath { get; set; } = string.Empty;

    public string PreviewType { get; set; } = string.Empty;

    public string? UploadedBy { get; set; }

    public DateTimeOffset UploadedAt { get; set; }

    public WorkComment Comment { get; set; } = null!;
}

public sealed class WorkAttachment : SoftDeletableAuditableEntity
{
    public Guid WorkItemId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public string StoragePath { get; set; } = string.Empty;

    public int Version { get; set; } = 1;

    public string? Description { get; set; }

    public WorkItem WorkItem { get; set; } = null!;
}

public sealed class WorkActivity
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public Guid? WorkItemId { get; set; }

    public Guid? UserId { get; set; }

    public WorkActivityType ActivityType { get; set; }

    public WorkActivityCategory Category { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? FieldName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Project Project { get; set; } = null!;

    public WorkItem? WorkItem { get; set; }

    public UserProfile? User { get; set; }
}

public sealed class WorkSavedFilter : SoftDeletableAuditableEntity
{
    public Guid? ProjectId { get; set; }

    public Guid? OwnerUserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string QueryJson { get; set; } = string.Empty;

    public bool IsShared { get; set; }

    public Project? Project { get; set; }
}

public enum WorkItemLinkType
{
    Duplicate = 1,
    Blocks = 2,
    BlockedBy = 3,
    RelatesTo = 4,
    DependsOn = 5
}

public enum WorkActivityType
{
    Created = 1,
    Updated = 2,
    StatusChanged = 3,
    AssignmentChanged = 4,
    PriorityChanged = 5,
    DescriptionChanged = 6,
    CommentAdded = 7,
    CommentUpdated = 8,
    CommentDeleted = 9,
    AttachmentAdded = 10,
    AttachmentUpdated = 11,
    AttachmentDeleted = 12,
    WorkflowTransitioned = 13,
    Linked = 14,
    TitleChanged = 15,
    ReporterChanged = 16,
    DueDateChanged = 17,
    EstimatedHoursChanged = 18,
    StoryPointsChanged = 19,
    LabelAdded = 20,
    LabelRemoved = 21,
    WatcherAdded = 22,
    WatcherRemoved = 23,
    ReplyAdded = 24,
    CommentEdited = 25,
    ReactionAdded = 26,
    ReactionRemoved = 27,
    MentionAdded = 28,
    MentionRemoved = 29,
    Pinned = 30,
    Unpinned = 31,
    Resolved = 32,
    Reopened = 33,
    ProjectMembershipChanged = 34
}

public enum WorkActivityCategory
{
    System = 1,
    Comments = 2,
    Assignments = 3,
    StatusChanges = 4,
    Priority = 5,
    Attachments = 6,
    Mentions = 7,
    Reactions = 8,
    ProjectMembership = 9
}
