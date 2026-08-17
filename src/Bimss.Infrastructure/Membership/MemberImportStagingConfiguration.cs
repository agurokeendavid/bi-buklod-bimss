using Bimss.Domain.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Membership;

public class MemberImportStagingConfiguration : IEntityTypeConfiguration<MemberImportStaging>
{
    public void Configure(EntityTypeBuilder<MemberImportStaging> builder)
    {
        builder.ToTable("MemberImportStaging");

        builder.HasKey(row => row.Id);

        // The raw source-value properties (LastName, FirstName, ... down to
        // PrivacyConsentRaw) are left unconfigured deliberately: EF Core maps
        // nullable string properties to nvarchar(max) by default, and this
        // table exists to capture whatever the legacy spreadsheet contains
        // before validation (BIMSS-035) runs. Constraining their length here
        // would risk silently truncating migration data.

        builder.Property(row => row.RowNumber).IsRequired();

        builder.Property(row => row.ValidationStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(row => row.MatchStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        // No two staging rows within the same batch share a source row
        // number.
        builder.HasIndex(row => new { row.ImportBatchId, row.RowNumber }).IsUnique();

        builder.HasIndex(row => row.MatchedMemberId);

        // A member is promoted from at most one staging row.
        builder.HasIndex(row => row.PromotedMemberId).IsUnique();

        builder.HasOne<ImportBatch>()
            .WithMany()
            .HasForeignKey(row => row.ImportBatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(row => row.MatchedMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(row => row.PromotedMemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
