using IntraFlow.Domain.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntraFlow.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.EntityId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ActionType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PerformedByUserId)
            .HasMaxLength(450);

        builder.Property(x => x.PerformedAt)
            .IsRequired();

        builder.Property(x => x.OldValuesJson);
        builder.Property(x => x.NewValuesJson);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.PerformedAt });
    }
}
