using Bimss.Domain.Membership;
using Bimss.Domain.Membership.ReferenceData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Membership;

public class MemberStatusHistoryConfiguration : IEntityTypeConfiguration<MemberStatusHistory>
{
    public void Configure(EntityTypeBuilder<MemberStatusHistory> builder)
    {
        builder.ToTable("MemberStatusHistory");

        builder.HasKey(history => history.Id);

        // Client-generated (Member.TransitionTo assigns Guid.NewGuid()), not
        // store-generated. Without this, EF's reachability fixup for a new
        // history row appended to an already-tracked Member's StatusHistory
        // collection sees a non-default key and assumes the row already
        // exists, tracking it as Modified instead of Added and failing on
        // save ("entity does not exist in the store").
        builder.Property(history => history.Id).ValueGeneratedNever();

        builder.Property(history => history.FromStatus)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(history => history.ToStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(history => history.Remarks)
            .HasMaxLength(1000);

        builder.HasOne<MemberStatusReason>()
            .WithMany()
            .HasForeignKey(history => history.ReasonId)
            .OnDelete(DeleteBehavior.Restrict);

        // No FK to AspNetUsers: status history must outlive the identity it
        // references, same reasoning as AuditEventConfiguration.
        builder.HasIndex(history => history.MemberId);
        builder.HasIndex(history => history.ActorUserId);
        builder.HasIndex(history => history.OccurredAtUtc);
    }
}
