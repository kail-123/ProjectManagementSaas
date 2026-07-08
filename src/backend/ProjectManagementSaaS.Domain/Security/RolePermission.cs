namespace ProjectManagementSaaS.Domain.Security;

public sealed class RolePermission
{
    public Guid RoleId { get; set; }

    public Guid PermissionId { get; set; }

    public DateTimeOffset AssignedOn { get; set; }

    public string? AssignedBy { get; set; }

    public Permission Permission { get; set; } = null!;
}
