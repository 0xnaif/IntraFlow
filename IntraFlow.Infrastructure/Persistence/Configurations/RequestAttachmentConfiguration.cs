using IntraFlow.Domain.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntraFlow.Infrastructure.Persistence.Configurations;

public class RequestAttachmentConfiguration : IEntityTypeConfiguration<RequestAttachment>
{
    public void Configure(EntityTypeBuilder<RequestAttachment> builder)
    {
        builder.ToTable("RequestAttachments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.FileSizeBytes)
            .IsRequired();

        builder.Property(x => x.FileData)
            .IsRequired();

        builder.Property(x => x.UploadedByUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.UploadedAt)
            .IsRequired();

        builder.HasOne<Request>()
            .WithMany()
            .HasForeignKey(x => x.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.RequestId);
    }
}
