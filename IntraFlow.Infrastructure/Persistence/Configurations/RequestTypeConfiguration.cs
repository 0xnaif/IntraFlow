using IntraFlow.Domain.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntraFlow.Infrastructure.Persistence.Configurations;

public class RequestTypeConfiguration : IEntityTypeConfiguration<RequestType>
{
    public void Configure(EntityTypeBuilder<RequestType> builder)
    {
        builder.ToTable("RequestTypes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive).IsRequired();

        builder.Property(x => x.DefaultApproverUserId)
            .HasMaxLength(450);

        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
