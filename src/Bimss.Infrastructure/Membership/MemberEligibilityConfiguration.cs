using Bimss.Domain.Membership;
using Bimss.Domain.Membership.ReferenceData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Membership;

public class MemberEligibilityConfiguration : IEntityTypeConfiguration<MemberEligibility>
{
    public void Configure(EntityTypeBuilder<MemberEligibility> builder)
    {
        builder.ToTable("MemberEligibilities");

        builder.HasKey(eligibility => eligibility.Id);

        builder.Property(eligibility => eligibility.Details)
            .HasMaxLength(500);

        // A member can hold more than one eligibility, so MemberId is
        // indexed for lookups but not unique.
        builder.HasIndex(eligibility => eligibility.MemberId);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(eligibility => eligibility.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<EligibilityType>()
            .WithMany()
            .HasForeignKey(eligibility => eligibility.EligibilityTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
