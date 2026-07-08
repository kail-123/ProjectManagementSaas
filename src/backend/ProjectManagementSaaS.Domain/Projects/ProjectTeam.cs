using ProjectManagementSaaS.Domain.Organizations;

namespace ProjectManagementSaaS.Domain.Projects;

public sealed class ProjectTeam
{
    public Guid ProjectId { get; set; }

    public Guid TeamId { get; set; }

    public DateTimeOffset AddedOn { get; set; }

    public string? AddedBy { get; set; }

    public Project Project { get; set; } = null!;

    public Team Team { get; set; } = null!;
}
