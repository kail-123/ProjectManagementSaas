using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementSaaS.Domain.Projects;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Configurations;

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects", "project");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Name).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.Code).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(2048);
        builder.Property(entity => entity.EstimatedBudget).HasPrecision(18, 2);
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.Priority).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.Visibility).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(256);

        builder.HasOne(entity => entity.Organization)
            .WithMany()
            .HasForeignKey(entity => entity.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Client)
            .WithMany()
            .HasForeignKey(entity => entity.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.ProjectManager)
            .WithMany()
            .HasForeignKey(entity => entity.ProjectManagerUserProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.OrganizationId, entity.Code })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(entity => entity.ClientId);
        builder.HasIndex(entity => entity.ProjectManagerUserProfileId);
        builder.HasIndex(entity => entity.Status);
        builder.HasIndex(entity => entity.Priority);
        builder.HasIndex(entity => entity.Visibility);

        builder.HasQueryFilter(entity => !entity.IsDeleted);
    }
}
