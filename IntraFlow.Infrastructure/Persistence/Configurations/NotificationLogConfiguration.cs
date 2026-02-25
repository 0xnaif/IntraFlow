using IntraFlow.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntraFlow.Infrastructure.Persistence.Configurations;

public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("NotificationLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RecipientEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired();

        builder.Property(x => x.FailureReason).HasMaxLength(1000);

        builder.HasIndex(x => new { x.RequestId, x.EventType });
        builder.HasIndex(x => x.RecipientEmail);
    }
}
