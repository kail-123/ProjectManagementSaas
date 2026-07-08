using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementSaaS.Domain.Organizations;
using ProjectManagementSaaS.Domain.Work;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Configurations;

internal sealed class WorkItemTypeConfiguration : IEntityTypeConfiguration<WorkItemType>
{
    public void Configure(EntityTypeBuilder<WorkItemType> builder)
    {
        builder.ToTable("WorkItemTypes", "work");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Name).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Code).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Icon).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Color).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(1024);
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(256);
        builder.HasOne(entity => entity.Workflow).WithMany().HasForeignKey(entity => entity.WorkflowId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => entity.Code).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasQueryFilter(entity => !entity.IsDeleted);
    }
}

internal sealed class WorkWorkflowConfiguration : IEntityTypeConfiguration<WorkWorkflow>
{
    public void Configure(EntityTypeBuilder<WorkWorkflow> builder)
    {
        builder.ToTable("Workflows", "work");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Name).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Code).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(1024);
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(256);
        builder.HasIndex(entity => entity.Code).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasQueryFilter(entity => !entity.IsDeleted);
    }
}

internal sealed class WorkWorkflowStatusConfiguration : IEntityTypeConfiguration<WorkWorkflowStatus>
{
    public void Configure(EntityTypeBuilder<WorkWorkflowStatus> builder)
    {
        builder.ToTable("WorkflowStatuses", "work");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Name).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Code).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Color).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(256);
        builder.HasOne(entity => entity.Workflow).WithMany(workflow => workflow.Statuses).HasForeignKey(entity => entity.WorkflowId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(entity => new { entity.WorkflowId, entity.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(entity => new { entity.WorkflowId, entity.SortOrder });
        builder.HasQueryFilter(entity => !entity.IsDeleted && !entity.Workflow.IsDeleted);
    }
}

internal sealed class WorkWorkflowTransitionConfiguration : IEntityTypeConfiguration<WorkWorkflowTransition>
{
    public void Configure(EntityTypeBuilder<WorkWorkflowTransition> builder)
    {
        builder.ToTable("WorkflowTransitions", "work");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Name).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(256);
        builder.HasOne(entity => entity.Workflow).WithMany(workflow => workflow.Transitions).HasForeignKey(entity => entity.WorkflowId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(entity => entity.FromStatus).WithMany().HasForeignKey(entity => entity.FromStatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.ToStatus).WithMany().HasForeignKey(entity => entity.ToStatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.WorkflowId, entity.FromStatusId, entity.ToStatusId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasQueryFilter(entity => !entity.IsDeleted && !entity.Workflow.IsDeleted);
    }
}

internal sealed class WorkPriorityConfiguration : IEntityTypeConfiguration<WorkPriority>
{
    public void Configure(EntityTypeBuilder<WorkPriority> builder)
    {
        builder.ToTable("Priorities", "work");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Name).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Code).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Color).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(256);
        builder.HasIndex(entity => entity.Code).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(entity => entity.SortOrder);
        builder.HasQueryFilter(entity => !entity.IsDeleted);
    }
}

internal sealed class WorkLabelConfiguration : IEntityTypeConfiguration<WorkLabel>
{
    public void Configure(EntityTypeBuilder<WorkLabel> builder)
    {
        builder.ToTable("Labels", "work");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Name).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Color).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(512);
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(256);
        builder.HasOne(entity => entity.Organization).WithMany().HasForeignKey(entity => entity.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.OrganizationId, entity.Name }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasQueryFilter(entity => !entity.IsDeleted && !entity.Organization.IsDeleted);
    }
}

internal sealed class WorkComponentConfiguration : IEntityTypeConfiguration<WorkComponent>
{
    public void Configure(EntityTypeBuilder<WorkComponent> builder)
    {
        builder.ToTable("Components", "work");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Name).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(512);
        builder.Property(entity => entity.Color).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(256);
        builder.HasOne(entity => entity.Project).WithMany().HasForeignKey(entity => entity.ProjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.ProjectId, entity.Name }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasQueryFilter(entity => !entity.IsDeleted && !entity.Project.IsDeleted);
    }
}

internal sealed class WorkItemConfiguration : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(EntityTypeBuilder<WorkItem> builder)
    {
        builder.ToTable("WorkItems", "work");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Title).HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(8192);
        builder.Property(entity => entity.AcceptanceCriteria).HasMaxLength(4096);
        builder.Property(entity => entity.EstimatedHours).HasPrecision(10, 2);
        builder.Property(entity => entity.LoggedHours).HasPrecision(10, 2);
        builder.Property(entity => entity.Environment).HasMaxLength(256);
        builder.Property(entity => entity.Severity).HasMaxLength(64);
        builder.Property(entity => entity.RowVersion).IsRowVersion();
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(256);
        builder.HasOne(entity => entity.Project).WithMany().HasForeignKey(entity => entity.ProjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.ParentWorkItem).WithMany(entity => entity.Children).HasForeignKey(entity => entity.ParentWorkItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Type).WithMany().HasForeignKey(entity => entity.WorkItemTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Status).WithMany().HasForeignKey(entity => entity.WorkflowStatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Priority).WithMany().HasForeignKey(entity => entity.PriorityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Assignee).WithMany().HasForeignKey(entity => entity.AssigneeUserProfileId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Reporter).WithMany().HasForeignKey(entity => entity.ReporterUserProfileId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => entity.ProjectId);
        builder.HasIndex(entity => entity.ParentWorkItemId);
        builder.HasIndex(entity => entity.WorkItemTypeId);
        builder.HasIndex(entity => entity.WorkflowStatusId);
        builder.HasIndex(entity => entity.PriorityId);
        builder.HasIndex(entity => entity.AssigneeUserProfileId);
        builder.HasIndex(entity => entity.ReporterUserProfileId);
        builder.HasIndex(entity => entity.DueDate);
        builder.HasQueryFilter(entity => !entity.IsDeleted && !entity.Project.IsDeleted);
    }
}

internal sealed class WorkItemLabelConfiguration : IEntityTypeConfiguration<WorkItemLabel>
{
    public void Configure(EntityTypeBuilder<WorkItemLabel> builder)
    {
        builder.ToTable("WorkItemLabels", "work");
        builder.HasKey(entity => new { entity.WorkItemId, entity.LabelId });
        builder.HasOne(entity => entity.WorkItem).WithMany(workItem => workItem.Labels).HasForeignKey(entity => entity.WorkItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(entity => entity.Label).WithMany().HasForeignKey(entity => entity.LabelId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(entity => !entity.WorkItem.IsDeleted && !entity.Label.IsDeleted);
    }
}

internal sealed class WorkItemComponentConfiguration : IEntityTypeConfiguration<WorkItemComponent>
{
    public void Configure(EntityTypeBuilder<WorkItemComponent> builder)
    {
        builder.ToTable("WorkItemComponents", "work");
        builder.HasKey(entity => new { entity.WorkItemId, entity.ComponentId });
        builder.HasOne(entity => entity.WorkItem).WithMany(workItem => workItem.Components).HasForeignKey(entity => entity.WorkItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(entity => entity.Component).WithMany().HasForeignKey(entity => entity.ComponentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(entity => !entity.WorkItem.IsDeleted && !entity.Component.IsDeleted);
    }
}

internal sealed class WorkItemWatcherConfiguration : IEntityTypeConfiguration<WorkItemWatcher>
{
    public void Configure(EntityTypeBuilder<WorkItemWatcher> builder)
    {
        builder.ToTable("WorkItemWatchers", "work");
        builder.HasKey(entity => new { entity.WorkItemId, entity.UserProfileId });
        builder.Property(entity => entity.AddedBy).HasMaxLength(256);
        builder.HasOne(entity => entity.WorkItem).WithMany(workItem => workItem.Watchers).HasForeignKey(entity => entity.WorkItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(entity => entity.UserProfile).WithMany().HasForeignKey(entity => entity.UserProfileId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(entity => !entity.WorkItem.IsDeleted && !entity.UserProfile.IsDeleted);
    }
}

internal sealed class WorkItemLinkConfiguration : IEntityTypeConfiguration<WorkItemLink>
{
    public void Configure(EntityTypeBuilder<WorkItemLink> builder)
    {
        builder.ToTable("WorkItemLinks", "work");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.LinkType).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(512);
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(256);
        builder.HasOne(entity => entity.SourceWorkItem).WithMany(workItem => workItem.OutboundLinks).HasForeignKey(entity => entity.SourceWorkItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.TargetWorkItem).WithMany(workItem => workItem.InboundLinks).HasForeignKey(entity => entity.TargetWorkItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.SourceWorkItemId, entity.TargetWorkItemId, entity.LinkType }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasQueryFilter(entity => !entity.IsDeleted && !entity.SourceWorkItem.IsDeleted && !entity.TargetWorkItem.IsDeleted);
    }
}

internal sealed class WorkCommentConfiguration : IEntityTypeConfiguration<WorkComment>
{
    public void Configure(EntityTypeBuilder<WorkComment> builder)
    {
        builder.ToTable("Comments", "work");
        builder.HasKey(entity => entity.Id);
        builder.Ignore(entity => entity.BodyMarkdown);
        builder.Property(entity => entity.Message).HasMaxLength(8192).IsRequired();
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(256);
        builder.HasOne(entity => entity.WorkItem).WithMany(workItem => workItem.Comments).HasForeignKey(entity => entity.WorkItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(entity => entity.ParentComment).WithMany(comment => comment.Replies).HasForeignKey(entity => entity.ParentCommentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Author).WithMany().HasPrincipalKey(entity => entity.UserId).HasForeignKey(entity => entity.AuthorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => entity.WorkItemId);
        builder.HasIndex(entity => new { entity.WorkItemId, entity.ParentCommentId, entity.CreatedOn });
        builder.HasIndex(entity => new { entity.WorkItemId, entity.Pinned, entity.CreatedOn });
        builder.HasIndex(entity => new { entity.WorkItemId, entity.Resolved, entity.CreatedOn });
        builder.HasQueryFilter(entity => !entity.WorkItem.IsDeleted);
    }
}

internal sealed class WorkCommentMentionConfiguration : IEntityTypeConfiguration<WorkCommentMention>
{
    public void Configure(EntityTypeBuilder<WorkCommentMention> builder)
    {
        builder.ToTable("CommentMentions", "work");
        builder.HasKey(entity => entity.Id);
        builder.HasOne(entity => entity.Comment).WithMany(comment => comment.Mentions).HasForeignKey(entity => entity.CommentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(entity => entity.MentionedUser).WithMany().HasPrincipalKey(entity => entity.UserId).HasForeignKey(entity => entity.MentionedUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.MentionedByUser).WithMany().HasPrincipalKey(entity => entity.UserId).HasForeignKey(entity => entity.MentionedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.CommentId, entity.MentionedUserId }).IsUnique();
        builder.HasIndex(entity => entity.MentionedUserId);
        builder.HasIndex(entity => entity.MentionedByUserId);
        builder.HasQueryFilter(entity => !entity.Comment.WorkItem.IsDeleted && !entity.MentionedUser.IsDeleted && entity.MentionedUser.IsActive);
    }
}

internal sealed class WorkCommentReactionConfiguration : IEntityTypeConfiguration<WorkCommentReaction>
{
    public void Configure(EntityTypeBuilder<WorkCommentReaction> builder)
    {
        builder.ToTable("CommentReactions", "work");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Emoji).HasMaxLength(16).IsRequired();
        builder.HasOne(entity => entity.Comment).WithMany(comment => comment.Reactions).HasForeignKey(entity => entity.CommentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(entity => entity.User).WithMany().HasPrincipalKey(entity => entity.UserId).HasForeignKey(entity => entity.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.CommentId, entity.UserId, entity.Emoji }).IsUnique();
        builder.HasIndex(entity => new { entity.CommentId, entity.Emoji });
        builder.HasQueryFilter(entity => !entity.Comment.WorkItem.IsDeleted && !entity.User.IsDeleted && entity.User.IsActive);
    }
}

internal sealed class WorkCommentReadConfiguration : IEntityTypeConfiguration<WorkCommentRead>
{
    public void Configure(EntityTypeBuilder<WorkCommentRead> builder)
    {
        builder.ToTable("CommentReads", "work");
        builder.HasKey(entity => entity.Id);
        builder.HasOne(entity => entity.Comment).WithMany(comment => comment.Reads).HasForeignKey(entity => entity.CommentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(entity => entity.User).WithMany().HasPrincipalKey(entity => entity.UserId).HasForeignKey(entity => entity.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.CommentId, entity.UserId }).IsUnique();
        builder.HasIndex(entity => new { entity.UserId, entity.ReadAt });
        builder.HasQueryFilter(entity => !entity.Comment.WorkItem.IsDeleted && !entity.User.IsDeleted && entity.User.IsActive);
    }
}

internal sealed class WorkCommentHistoryConfiguration : IEntityTypeConfiguration<WorkCommentHistory>
{
    public void Configure(EntityTypeBuilder<WorkCommentHistory> builder)
    {
        builder.ToTable("CommentHistory", "work");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.EditedByName).HasMaxLength(256);
        builder.Property(entity => entity.PreviousMessage).HasMaxLength(8192).IsRequired();
        builder.Property(entity => entity.NewMessage).HasMaxLength(8192).IsRequired();
        builder.HasOne(entity => entity.Comment).WithMany(comment => comment.History).HasForeignKey(entity => entity.CommentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(entity => entity.EditedByProfile).WithMany().HasPrincipalKey(entity => entity.UserId).HasForeignKey(entity => entity.EditedBy).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.CommentId, entity.EditedAt });
        builder.HasQueryFilter(entity => !entity.Comment.WorkItem.IsDeleted);
    }
}

internal sealed class WorkCommentAttachmentConfiguration : IEntityTypeConfiguration<WorkCommentAttachment>
{
    public void Configure(EntityTypeBuilder<WorkCommentAttachment> builder)
    {
        builder.ToTable("CommentAttachments", "work");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.FileName).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.ContentType).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.StoragePath).HasMaxLength(1024).IsRequired();
        builder.Property(entity => entity.PreviewType).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.UploadedBy).HasMaxLength(256);
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(256);
        builder.HasOne(entity => entity.Comment).WithMany(comment => comment.Attachments).HasForeignKey(entity => entity.CommentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(entity => new { entity.CommentId, entity.FileName }).HasFilter("[IsDeleted] = 0");
        builder.HasQueryFilter(entity => !entity.IsDeleted && !entity.Comment.WorkItem.IsDeleted);
    }
}

internal sealed class WorkAttachmentConfiguration : IEntityTypeConfiguration<WorkAttachment>
{
    public void Configure(EntityTypeBuilder<WorkAttachment> builder)
    {
        builder.ToTable("Attachments", "work");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.FileName).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.ContentType).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.StoragePath).HasMaxLength(1024).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(512);
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(256);
        builder.HasOne(entity => entity.WorkItem).WithMany(workItem => workItem.Attachments).HasForeignKey(entity => entity.WorkItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(entity => new { entity.WorkItemId, entity.Version, entity.FileName });
        builder.HasQueryFilter(entity => !entity.IsDeleted && !entity.WorkItem.IsDeleted);
    }
}

internal sealed class WorkActivityConfiguration : IEntityTypeConfiguration<WorkActivity>
{
    public void Configure(EntityTypeBuilder<WorkActivity> builder)
    {
        builder.ToTable("ActivityLogs", "work");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.ActivityType).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Category).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(1024).IsRequired();
        builder.Property(entity => entity.FieldName).HasMaxLength(128);
        builder.Property(entity => entity.OldValue).HasMaxLength(2048);
        builder.Property(entity => entity.NewValue).HasMaxLength(2048);
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.HasOne(entity => entity.Project).WithMany().HasForeignKey(entity => entity.ProjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.WorkItem).WithMany().HasForeignKey(entity => entity.WorkItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.User).WithMany().HasPrincipalKey(entity => entity.UserId).HasForeignKey(entity => entity.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.WorkItemId, entity.CreatedAt });
        builder.HasIndex(entity => new { entity.ProjectId, entity.CreatedAt });
        builder.HasIndex(entity => new { entity.Category, entity.CreatedAt });
        builder.HasQueryFilter(entity => !entity.Project.IsDeleted && (entity.WorkItemId == null || !entity.WorkItem!.IsDeleted));
    }
}

internal sealed class WorkSavedFilterConfiguration : IEntityTypeConfiguration<WorkSavedFilter>
{
    public void Configure(EntityTypeBuilder<WorkSavedFilter> builder)
    {
        builder.ToTable("SavedFilters", "work");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Name).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.QueryJson).HasMaxLength(4096).IsRequired();
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(256);
        builder.HasOne(entity => entity.Project).WithMany().HasForeignKey(entity => entity.ProjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.OwnerUserId, entity.Name }).HasFilter("[IsDeleted] = 0");
        builder.HasQueryFilter(entity => !entity.IsDeleted);
    }
}
