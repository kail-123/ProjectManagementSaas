using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementSaaS.Domain.Organizations;
using ProjectManagementSaaS.Infrastructure.Persistence.Entities;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Configurations;

internal sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles", "organization");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.FirstName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.LastName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.EmployeeCode).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Designation).HasMaxLength(256);
        builder.Property(entity => entity.ProfilePhoto).HasMaxLength(1024);
        builder.Property(entity => entity.Phone).HasMaxLength(64);
        builder.Property(entity => entity.TimeZone).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Skills).HasMaxLength(2048);
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(256);

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<UserProfile>(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.Organization)
            .WithMany()
            .HasForeignKey(entity => entity.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Department)
            .WithMany()
            .HasForeignKey(entity => entity.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Team)
            .WithMany()
            .HasForeignKey(entity => entity.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => entity.UserId).IsUnique();
        builder.HasIndex(entity => entity.EmployeeCode).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(entity => entity.IsActive);

        builder.HasQueryFilter(entity => !entity.IsDeleted);
    }
}
