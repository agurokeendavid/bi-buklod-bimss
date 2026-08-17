using Bimss.Domain.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Membership;

public class ImportValidationErrorConfiguration : IEntityTypeConfiguration<ImportValidationError>
{
    public void Configure(EntityTypeBuilder<ImportValidationError> builder)
    {
        builder.ToTable("ImportValidationErrors");

        builder.HasKey(error => error.Id);

        builder.Property(error => error.FieldName)
            .HasMaxLength(200);

        builder.Property(error => error.Severity)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(error => error.Message)
            .HasMaxLength(2000)
            .IsRequired();

        builder.HasIndex(error => error.ImportBatchId);

        builder.HasIndex(error => error.MemberImportStagingId);

        builder.HasOne<ImportBatch>()
            .WithMany()
            .HasForeignKey(error => error.ImportBatchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict, not Cascade: ImportBatchId above already gives SQL Server
        // one cascade path from ImportBatches down to ImportValidationErrors.
        // A second cascade path through MemberImportStaging (ImportBatch ->
        // MemberImportStaging -> ImportValidationErrors) is what SQL Server
        // rejects as "multiple cascade paths" when creating the FK — staging
        // rows are only ever deleted via their batch, and that batch delete
        // already removes their validation errors through the direct FK
        // above, so no delete path is lost by not cascading here too.
        builder.HasOne<MemberImportStaging>()
            .WithMany()
            .HasForeignKey(error => error.MemberImportStagingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
