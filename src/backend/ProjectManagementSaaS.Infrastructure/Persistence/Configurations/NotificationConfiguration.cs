using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementSaaS.Domain.Notifications;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Configurations;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications", "notifications");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Title).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.Message).HasMaxLength(1000).IsRequired();
        builder.Property(entity => entity.NotificationType).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Priority).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.ActionUrl).HasMaxLength(512);
        builder.Property(entity => entity.Icon).HasMaxLength(64);
        builder.Property(entity => entity.CreatedBy).HasMaxLength(256);

        builder.HasOne(entity => entity.User)
            .WithMany()
            .HasPrincipalKey(entity => entity.UserId)
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Project)
            .WithMany()
            .HasForeignKey(entity => entity.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.WorkItem)
            .WithMany()
            .HasForeignKey(entity => entity.WorkItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.UserId, entity.IsRead, entity.CreatedAt });
        builder.HasIndex(entity => new { entity.UserId, entity.CreatedAt });
        builder.HasIndex(entity => new { entity.ProjectId, entity.CreatedAt });
        builder.HasIndex(entity => new { entity.NotificationType, entity.CreatedAt });
        builder.HasIndex(entity => entity.ExpiresAt);
        builder.HasQueryFilter(entity => entity.User == null || !entity.User.IsDeleted);
    }
}

internal sealed class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("NotificationPreferences", "notifications");

        builder.HasKey(entity => entity.UserId);

        builder.Property(entity => entity.UpdatedBy).HasMaxLength(256);

        builder.HasOne(entity => entity.User)
            .WithMany()
            .HasPrincipalKey(entity => entity.UserId)
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(entity => entity.User == null || !entity.User.IsDeleted);
    }
}

internal sealed class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
    {
        builder.ToTable("NotificationDeliveries", "notifications");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Channel).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.Error).HasMaxLength(1000);

        builder.HasOne(entity => entity.Notification)
            .WithMany()
            .HasForeignKey(entity => entity.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(entity => entity.NotificationId);
        builder.HasIndex(entity => new { entity.UserId, entity.CreatedAt });
        builder.HasIndex(entity => new { entity.Status, entity.CreatedAt });
        builder.HasQueryFilter(entity => entity.Notification.User == null || !entity.Notification.User.IsDeleted);
    }
}

internal sealed class NotificationOutboxConfiguration : IEntityTypeConfiguration<NotificationOutbox>
{
    public void Configure(EntityTypeBuilder<NotificationOutbox> builder)
    {
        builder.ToTable("NotificationOutbox", "notifications");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.EventType).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Payload).IsRequired();
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.LastError).HasMaxLength(1000);

        builder.HasIndex(entity => new { entity.Status, entity.CreatedAt });
    }
}
