using ProjectManagementSaaS.Domain.Common;

namespace ProjectManagementSaaS.Domain.Projects;

public sealed class ProjectSettings : IAuditableEntity
{
    public Guid ProjectId { get; set; }

    public bool EnableSprint { get; set; } = true;

    public bool EnableBacklog { get; set; } = true;

    public bool EnableKanban { get; set; } = true;

    public bool EnableTimeTracking { get; set; } = true;

    public bool EnableWiki { get; set; } = true;

    public bool EnableDocuments { get; set; } = true;

    public bool EnableApprovals { get; set; }

    public bool EnableLeaveRequests { get; set; }

    public bool EnableBugTracking { get; set; } = true;

    public bool EnableRiskRegister { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedOn { get; set; }

    public Project Project { get; set; } = null!;
}
