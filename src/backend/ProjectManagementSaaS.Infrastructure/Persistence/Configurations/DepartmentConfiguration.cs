using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementSaaS.Domain.Organizations;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Configurations;

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments", "organization");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Name).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.Code).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(1024);
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(256);

        builder.HasOne(entity => entity.Organization)
            .WithMany()
            .HasForeignKey(entity => entity.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.OrganizationId, entity.Code })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(entity => entity.IsActive);

        builder.HasQueryFilter(entity => !entity.IsDeleted);
    }
}
