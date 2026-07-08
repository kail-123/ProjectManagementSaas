using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementSaaS.Domain.Organizations;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations", "organization");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Name).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.Code).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Logo).HasMaxLength(1024);
        builder.Property(entity => entity.Website).HasMaxLength(512);
        builder.Property(entity => entity.Email).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.Phone).HasMaxLength(64);
        builder.Property(entity => entity.Address).HasMaxLength(512);
        builder.Property(entity => entity.City).HasMaxLength(128);
        builder.Property(entity => entity.State).HasMaxLength(128);
        builder.Property(entity => entity.Country).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.TimeZone).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Currency).HasMaxLength(3).IsRequired();
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(256);

        builder.HasIndex(entity => entity.Code).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(entity => entity.Email).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(entity => entity.IsActive);

        builder.HasQueryFilter(entity => !entity.IsDeleted);
    }
}
