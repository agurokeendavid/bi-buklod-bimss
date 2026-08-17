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

        builder.HasOne<MemberImportStaging>()
            .WithMany()
            .HasForeignKey(error => error.MemberImportStagingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
