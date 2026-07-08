using ProjectManagementSaaS.Domain.Organizations;

namespace ProjectManagementSaaS.Domain.Projects;

public sealed class ProjectMember
{
    public Guid ProjectId { get; set; }

    public Guid UserId { get; set; }

    public ProjectMemberRole RoleInProject { get; set; } = ProjectMemberRole.Developer;

    public DateTimeOffset JoinedDate { get; set; }

    public bool IsActive { get; set; } = true;

    public string? AddedBy { get; set; }

    public Project Project { get; set; } = null!;

    public UserProfile? UserProfile { get; set; }
}
