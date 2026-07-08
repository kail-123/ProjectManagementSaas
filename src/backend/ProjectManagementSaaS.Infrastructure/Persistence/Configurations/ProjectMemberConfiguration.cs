using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementSaaS.Domain.Projects;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Configurations;

internal sealed class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.ToTable("ProjectMembers", "project");

        builder.HasKey(entity => new { entity.ProjectId, entity.UserId });

        builder.Property(entity => entity.RoleInProject).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.AddedBy).HasMaxLength(256);

        builder.HasOne(entity => entity.Project)
            .WithMany(project => project.Members)
            .HasForeignKey(entity => entity.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.UserProfile)
            .WithMany()
            .HasPrincipalKey(entity => entity.UserId)
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => entity.UserId);
        builder.HasIndex(entity => entity.RoleInProject);
        builder.HasIndex(entity => entity.IsActive);

        builder.HasQueryFilter(entity => !entity.Project.IsDeleted && (entity.UserProfile == null || !entity.UserProfile.IsDeleted));
    }
}
