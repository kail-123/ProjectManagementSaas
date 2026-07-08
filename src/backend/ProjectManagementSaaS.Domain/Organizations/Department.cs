using ProjectManagementSaaS.Domain.Common;

namespace ProjectManagementSaaS.Domain.Organizations;

public sealed class Department : SoftDeletableAuditableEntity
{
    public Guid OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public Organization Organization { get; set; } = null!;
}
