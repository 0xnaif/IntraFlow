using IntraFlow.Domain.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Infrastructure.Persistence.Configurations;

public class RequestDecisionConfiguration : IEntityTypeConfiguration<RequestDecision>
{
    public void Configure(EntityTypeBuilder<RequestDecision> builder)
    {
        builder.ToTable("RequestDecisions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DecidedByUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.DecidedAt)
            .IsRequired();

        builder.Property(x => x.Decision)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.DecisionReason)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.RequestId).IsUnique();

        builder.HasOne<Request>()
            .WithOne()
            .HasForeignKey<RequestDecision>(x => x.RequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
