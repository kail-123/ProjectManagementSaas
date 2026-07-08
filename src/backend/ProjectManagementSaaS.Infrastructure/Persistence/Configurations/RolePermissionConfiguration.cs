using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementSaaS.Domain.Security;
using ProjectManagementSaaS.Infrastructure.Persistence.Entities;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Configurations;

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions", "security");

        builder.HasKey(entity => new { entity.RoleId, entity.PermissionId });

        builder.Property(entity => entity.AssignedBy).HasMaxLength(256);

        builder.HasOne<ApplicationRole>()
            .WithMany(role => role.RolePermissions)
            .HasForeignKey(entity => entity.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.Permission)
            .WithMany(permission => permission.RolePermissions)
            .HasForeignKey(entity => entity.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(entity => !entity.Permission.IsDeleted);
    }
}
