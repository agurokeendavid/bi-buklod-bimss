using Bimss.Domain.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Membership;

public class MemberUpdateRequestChangeConfiguration : IEntityTypeConfiguration<MemberUpdateRequestChange>
{
    public void Configure(EntityTypeBuilder<MemberUpdateRequestChange> builder)
    {
        builder.ToTable("MemberUpdateRequestChanges");

        builder.HasKey(change => change.Id);

        builder.Property(change => change.FieldName)
            .HasMaxLength(100)
            .IsRequired();

        // OldValue/NewValue stay unconstrained (nvarchar(max), EF's
        // default) — this row can hold any Member/MemberEmployment field's
        // value, and those vary widely in natural length (a date vs.
        // JoiningReason's free text); constraining here risks truncation.

        builder.HasIndex(change => change.MemberUpdateRequestId);
    }
}
