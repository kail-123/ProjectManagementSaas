using ProjectManagementSaaS.Domain.Common;

namespace ProjectManagementSaaS.Domain.Organizations;

public sealed class Team : SoftDeletableAuditableEntity
{
    public Guid DepartmentId { get; set; }

    public Guid? TeamLeadUserProfileId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public Department Department { get; set; } = null!;

    public UserProfile? TeamLead { get; set; }

    public ICollection<TeamMember> Members { get; } = new List<TeamMember>();
}
