using ProjectManagementSaaS.Domain.Common;

namespace ProjectManagementSaaS.Domain.Security;

public sealed class Permission : SoftDeletableAuditableEntity
{
    public string Module { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<RolePermission> RolePermissions { get; } = new List<RolePermission>();
}
