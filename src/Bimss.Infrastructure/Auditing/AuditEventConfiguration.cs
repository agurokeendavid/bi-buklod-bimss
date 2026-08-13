using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Auditing;

public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("AuditEvents");

        builder.HasKey(auditEvent => auditEvent.Id);

        builder.Property(auditEvent => auditEvent.Action)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(auditEvent => auditEvent.ObjectType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(auditEvent => auditEvent.ObjectId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(auditEvent => auditEvent.Result)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(auditEvent => auditEvent.Remarks)
            .HasMaxLength(1000);

        builder.Property(auditEvent => auditEvent.MetadataJson)
            .HasColumnType("nvarchar(max)");

        // No FK to AspNetUsers: audit records must outlive the identity they
        // reference (no cascading loss of audit history if a user is removed).
        builder.HasIndex(auditEvent => auditEvent.ActorUserId);
        builder.HasIndex(auditEvent => new { auditEvent.ObjectType, auditEvent.ObjectId });
        builder.HasIndex(auditEvent => auditEvent.TimestampUtc);
    }
}
