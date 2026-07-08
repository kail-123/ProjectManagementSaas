using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementSaaS.Domain.Projects;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Configurations;

internal sealed class ProjectSettingsConfiguration : IEntityTypeConfiguration<ProjectSettings>
{
    public void Configure(EntityTypeBuilder<ProjectSettings> builder)
    {
        builder.ToTable("ProjectSettings", "project");

        builder.HasKey(entity => entity.ProjectId);

        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);

        builder.HasOne(entity => entity.Project)
            .WithOne(project => project.Settings)
            .HasForeignKey<ProjectSettings>(entity => entity.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(entity => !entity.Project.IsDeleted);
    }
}
