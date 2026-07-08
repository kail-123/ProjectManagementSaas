using ProjectManagementSaaS.Domain.Common;
using ProjectManagementSaaS.Domain.Organizations;

namespace ProjectManagementSaaS.Domain.Projects;

public sealed class Project : SoftDeletableAuditableEntity
{
    public Guid OrganizationId { get; set; }

    public Guid? ClientId { get; set; }

    public Guid? ProjectManagerUserProfileId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public decimal? EstimatedBudget { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

    public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;

    public ProjectVisibility Visibility { get; set; } = ProjectVisibility.Organization;

    public Organization Organization { get; set; } = null!;

    public Client? Client { get; set; }

    public UserProfile? ProjectManager { get; set; }

    public ProjectSettings? Settings { get; set; }

    public ICollection<ProjectTeam> ProjectTeams { get; } = new List<ProjectTeam>();

    public ICollection<ProjectMember> Members { get; } = new List<ProjectMember>();
}
