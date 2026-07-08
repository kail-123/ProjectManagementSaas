using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementSaaS.Domain.Organizations;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Configurations;

internal sealed class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("TeamMembers", "organization");

        builder.HasKey(entity => new { entity.TeamId, entity.UserProfileId });

        builder.Property(entity => entity.AddedBy).HasMaxLength(256);

        builder.HasOne(entity => entity.Team)
            .WithMany(team => team.Members)
            .HasForeignKey(entity => entity.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.UserProfile)
            .WithMany()
            .HasForeignKey(entity => entity.UserProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(entity => !entity.Team.IsDeleted && !entity.UserProfile.IsDeleted);
    }
}
