using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementSaaS.Domain.Projects;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Configurations;

internal sealed class ProjectTeamConfiguration : IEntityTypeConfiguration<ProjectTeam>
{
    public void Configure(EntityTypeBuilder<ProjectTeam> builder)
    {
        builder.ToTable("ProjectTeams", "project");

        builder.HasKey(entity => new { entity.ProjectId, entity.TeamId });

        builder.Property(entity => entity.AddedBy).HasMaxLength(256);

        builder.HasOne(entity => entity.Project)
            .WithMany(project => project.ProjectTeams)
            .HasForeignKey(entity => entity.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.Team)
            .WithMany()
            .HasForeignKey(entity => entity.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => entity.TeamId);

        builder.HasQueryFilter(entity => !entity.Project.IsDeleted && !entity.Team.IsDeleted);
    }
}
