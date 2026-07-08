using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementSaaS.Domain.Security;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Configurations;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions", "security");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Module).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Name).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.Code).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(1024);
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(256);

        builder.HasIndex(entity => entity.Code).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(entity => entity.Module);
        builder.HasIndex(entity => entity.IsActive);

        builder.HasQueryFilter(entity => !entity.IsDeleted);
    }
}
