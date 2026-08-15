using Bimss.Domain.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Membership;

public class MemberPrivacyConsentConfiguration : IEntityTypeConfiguration<MemberPrivacyConsent>
{
    public void Configure(EntityTypeBuilder<MemberPrivacyConsent> builder)
    {
        builder.ToTable("MemberPrivacyConsents");

        builder.HasKey(consent => consent.Id);

        builder.Property(consent => consent.NoticeVersion)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(consent => consent.Source)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(consent => consent.ConsentGiven)
            .IsRequired();

        // A member accumulates one consent record per consent event
        // (re-consenting to a later notice version, withdrawing consent),
        // so MemberId is indexed for lookups but not unique.
        builder.HasIndex(consent => consent.MemberId);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(consent => consent.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
