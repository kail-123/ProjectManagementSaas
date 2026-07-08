using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementSaaS.Domain.Projects;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Configurations;

internal sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients", "project");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Name).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.Code).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.ContactPerson).HasMaxLength(256);
        builder.Property(entity => entity.Email).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.Phone).HasMaxLength(64);
        builder.Property(entity => entity.Mobile).HasMaxLength(64);
        builder.Property(entity => entity.GstNumber).HasMaxLength(32);
        builder.Property(entity => entity.Pan).HasMaxLength(32);
        builder.Property(entity => entity.BillingAddress).HasMaxLength(1024);
        builder.Property(entity => entity.ShippingAddress).HasMaxLength(1024);
        builder.Property(entity => entity.Country).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.State).HasMaxLength(128);
        builder.Property(entity => entity.City).HasMaxLength(128);
        builder.Property(entity => entity.Website).HasMaxLength(512);
        builder.Property(entity => entity.Notes).HasMaxLength(2048);
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
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
        builder.HasIndex(entity => new { entity.OrganizationId, entity.Email })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(entity => entity.Status);

        builder.HasQueryFilter(entity => !entity.IsDeleted);
    }
}
