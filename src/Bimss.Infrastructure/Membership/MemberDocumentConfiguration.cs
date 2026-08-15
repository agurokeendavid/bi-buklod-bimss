using Bimss.Domain.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Membership;

public class MemberDocumentConfiguration : IEntityTypeConfiguration<MemberDocument>
{
    public void Configure(EntityTypeBuilder<MemberDocument> builder)
    {
        builder.ToTable("MemberDocuments");

        builder.HasKey(document => document.Id);

        builder.Property(document => document.DocumentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(document => document.OriginalFileName)
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(document => document.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(document => document.StorageKey)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(document => document.FileSizeBytes)
            .IsRequired();

        builder.HasIndex(document => document.StorageKey).IsUnique();

        // A member can upload more than one document, so MemberId is
        // indexed for lookups but not unique.
        builder.HasIndex(document => document.MemberId);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(document => document.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
