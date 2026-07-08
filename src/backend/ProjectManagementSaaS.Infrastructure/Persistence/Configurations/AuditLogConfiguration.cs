using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementSaaS.Domain.Audit;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs", "audit");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.EntityName).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.EntityId).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Action).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.ChangedBy).HasMaxLength(256);

        builder.HasIndex(entity => new { entity.EntityName, entity.EntityId });
        builder.HasIndex(entity => entity.ChangedOn);
    }
}
