using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementSaaS.Infrastructure.Persistence.Entities;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Configurations;

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users", "identity");

        builder.Property(user => user.DisplayName)
            .HasMaxLength(256);

        builder.HasIndex(user => user.IsActive);
    }
}
