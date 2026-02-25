using IntraFlow.Domain.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Infrastructure.Persistence.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.ToTable("Requests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.CreatedByUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.AssignedApproverUserId)
            .HasMaxLength(450);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.LastUpdatedAt).IsRequired();

        builder.Property(x => x.SubmittedAt);
        builder.Property(x => x.InReviewAt);
        builder.Property(x => x.DecisionAt);
        builder.Property(x => x.CancelledAt);

        
        builder.HasOne<RequestType>()
            .WithMany()
            .HasForeignKey(x => x.RequestTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
        builder.HasIndex(x => new { x.RequestTypeId, x.CreatedAt });
        builder.HasIndex(x => new { x.CreatedByUserId, x.CreatedAt });
        builder.HasIndex(x => new { x.AssignedApproverUserId, x.Status });
    }
}
