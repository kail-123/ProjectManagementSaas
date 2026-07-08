using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSaaS.Application.Abstractions.Common;
using ProjectManagementSaaS.Application.Abstractions.Persistence;
using ProjectManagementSaaS.Domain.Audit;
using ProjectManagementSaaS.Domain.Common;
using ProjectManagementSaaS.Domain.Notifications;
using ProjectManagementSaaS.Domain.Organizations;
using ProjectManagementSaaS.Domain.Projects;
using ProjectManagementSaaS.Domain.Security;
using ProjectManagementSaaS.Domain.Work;
using ProjectManagementSaaS.Infrastructure.Persistence.Entities;
using System.Text.Json;

namespace ProjectManagementSaaS.Infrastructure.Persistence;

public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext, IUnitOfWork
{
    private readonly ICurrentUserService? _currentUserService;
    private readonly IDateTimeProvider? _dateTimeProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _dateTimeProvider = dateTimeProvider;
        _currentUserService = currentUserService;
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

    public DbSet<ProjectSettings> ProjectSettings => Set<ProjectSettings>();

    public DbSet<ProjectTeam> ProjectTeams => Set<ProjectTeam>();

    public DbSet<WorkItemType> WorkItemTypes => Set<WorkItemType>();

    public DbSet<WorkWorkflow> WorkWorkflows => Set<WorkWorkflow>();

    public DbSet<WorkWorkflowStatus> WorkWorkflowStatuses => Set<WorkWorkflowStatus>();

    public DbSet<WorkWorkflowTransition> WorkWorkflowTransitions => Set<WorkWorkflowTransition>();

    public DbSet<WorkPriority> WorkPriorities => Set<WorkPriority>();

    public DbSet<WorkLabel> WorkLabels => Set<WorkLabel>();

    public DbSet<WorkComponent> WorkComponents => Set<WorkComponent>();

    public DbSet<WorkItem> WorkItems => Set<WorkItem>();

    public DbSet<WorkItemLabel> WorkItemLabels => Set<WorkItemLabel>();

    public DbSet<WorkItemComponent> WorkItemComponents => Set<WorkItemComponent>();

    public DbSet<WorkItemWatcher> WorkItemWatchers => Set<WorkItemWatcher>();

    public DbSet<WorkItemLink> WorkItemLinks => Set<WorkItemLink>();

    public DbSet<WorkComment> WorkComments => Set<WorkComment>();

    public DbSet<WorkCommentMention> WorkCommentMentions => Set<WorkCommentMention>();

    public DbSet<WorkCommentReaction> WorkCommentReactions => Set<WorkCommentReaction>();

    public DbSet<WorkCommentRead> WorkCommentReads => Set<WorkCommentRead>();

    public DbSet<WorkCommentHistory> WorkCommentHistory => Set<WorkCommentHistory>();

    public DbSet<WorkCommentAttachment> WorkCommentAttachments => Set<WorkCommentAttachment>();

    public DbSet<WorkAttachment> WorkAttachments => Set<WorkAttachment>();

    public DbSet<WorkActivity> WorkActivities => Set<WorkActivity>();

    public DbSet<WorkSavedFilter> WorkSavedFilters => Set<WorkSavedFilter>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();

    public DbSet<NotificationDelivery> NotificationDeliveries => Set<NotificationDelivery>();

    public DbSet<NotificationOutbox> NotificationOutbox => Set<NotificationOutbox>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDelete();

        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditAndSoftDelete()
    {
        var now = _dateTimeProvider?.UtcNow ?? DateTimeOffset.UtcNow;
        var actor = _currentUserService?.UserName ?? _currentUserService?.UserId?.ToString() ?? "system";
        var auditLogs = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries().Where(entry => entry.Entity is not AuditLog))
        {
            if (entry.Entity is ISoftDeletable softDeletable && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                softDeletable.IsDeleted = true;
                softDeletable.DeletedOn = now;
                softDeletable.DeletedBy = actor;
            }

            if (entry.Entity is IAuditableEntity auditable)
            {
                if (entry.State == EntityState.Added)
                {
                    auditable.CreatedOn = auditable.CreatedOn == default ? now : auditable.CreatedOn;
                    auditable.CreatedBy ??= actor;
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditable.UpdatedOn = now;
                    auditable.UpdatedBy = actor;
                }
            }

            if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            {
                auditLogs.Add(CreateAuditLog(entry, now, actor));
            }
        }

        if (auditLogs.Count > 0)
        {
            AuditLogs.AddRange(auditLogs);
        }
    }

    private static AuditLog CreateAuditLog(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, DateTimeOffset now, string actor)
    {
        var entityId = entry.Properties.FirstOrDefault(property => property.Metadata.IsPrimaryKey())?.CurrentValue?.ToString() ?? string.Empty;
        var changes = entry.Properties
            .Where(property => entry.State == EntityState.Added || property.IsModified)
            .ToDictionary(
                property => property.Metadata.Name,
                property => new
                {
                    Original = entry.State == EntityState.Added ? null : property.OriginalValue,
                    Current = property.CurrentValue
                },
                StringComparer.Ordinal);

        return new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityName = entry.Metadata.ClrType.Name,
            EntityId = entityId,
            Action = entry.State.ToString(),
            Changes = JsonSerializer.Serialize(changes),
            ChangedBy = actor,
            ChangedOn = now
        };
    }
}
