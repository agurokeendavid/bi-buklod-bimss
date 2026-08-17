using Bimss.Domain.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Membership;

public class MemberUpdateRequestConfiguration : IEntityTypeConfiguration<MemberUpdateRequest>
{
    public void Configure(EntityTypeBuilder<MemberUpdateRequest> builder)
    {
        builder.ToTable("MemberUpdateRequests");

        builder.HasKey(request => request.Id);

        builder.Property(request => request.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(request => request.ReviewRemarks)
            .HasMaxLength(2000);

        builder.HasIndex(request => request.MemberId);

        // No FK from SubmittedByUserId/ReviewedByUserId to AspNetUsers —
        // same reasoning as MemberStatusHistory.ActorUserId: an audit-
        // relevant actor reference must outlive the identity it points to.
        builder.HasIndex(request => request.SubmittedByUserId);
        builder.HasIndex(request => request.ReviewedByUserId);

        builder.Navigation(request => request.Changes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(request => request.Changes)
            .WithOne()
            .HasForeignKey(change => change.MemberUpdateRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // A request belongs wholly to its member — same cascade reasoning
        // as MemberStatusHistory (an audit trail of that member).
        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(request => request.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
