using Bimss.Domain.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Membership;

public class MemberAddressConfiguration : IEntityTypeConfiguration<MemberAddress>
{
    public void Configure(EntityTypeBuilder<MemberAddress> builder)
    {
        builder.ToTable("MemberAddresses");

        builder.HasKey(address => address.Id);

        builder.Property(address => address.AddressType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(address => address.AddressLine)
            .HasMaxLength(500)
            .IsRequired();

        // One address row per (member, type) — e.g. one Present, one Permanent.
        builder.HasIndex(address => new { address.MemberId, address.AddressType }).IsUnique();

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(address => address.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
