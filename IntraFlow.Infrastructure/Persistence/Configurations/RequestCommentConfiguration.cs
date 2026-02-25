using IntraFlow.Domain.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Infrastructure.Persistence.Configurations;

public class RequestCommentConfiguration : IEntityTypeConfiguration<RequestComment>
{
    public void Configure(EntityTypeBuilder<RequestComment> builder)
    {
        builder.ToTable("RequestComments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AuthorUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.Body)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // How to read this? Or who comes first and later?
        builder.HasOne<Request>()
            .WithMany()
            .HasForeignKey(x => x.RequestId)
            .OnDelete(DeleteBehavior.Cascade);
        // What is the point of this? Or how it really works?
        builder.HasIndex(x => new { x.RequestId, x.CreatedAt });
    }
}
