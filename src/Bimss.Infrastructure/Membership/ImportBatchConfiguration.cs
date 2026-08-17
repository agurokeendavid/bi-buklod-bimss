using Bimss.Domain.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Membership;

public class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> builder)
    {
        builder.ToTable("ImportBatches");

        builder.HasKey(batch => batch.Id);

        builder.Property(batch => batch.FileName)
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(batch => batch.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(batch => batch.Remarks)
            .HasMaxLength(2000);

        // No FK to AspNetUsers — same reasoning as AuditEventConfiguration/
        // MemberStatusHistoryConfiguration: an actor reference used for audit
        // must outlive the identity row it points to.
        builder.HasIndex(batch => batch.UploadedByUserId);
    }
}
