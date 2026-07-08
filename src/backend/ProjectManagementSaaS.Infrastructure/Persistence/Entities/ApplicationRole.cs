using Microsoft.AspNetCore.Identity;
using ProjectManagementSaaS.Domain.Security;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Entities;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public ICollection<RolePermission> RolePermissions { get; } = new List<RolePermission>();
}
