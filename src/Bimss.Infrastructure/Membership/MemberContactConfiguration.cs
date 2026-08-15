using Bimss.Domain.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Membership;

public class MemberContactConfiguration : IEntityTypeConfiguration<MemberContact>
{
    public void Configure(EntityTypeBuilder<MemberContact> builder)
    {
        builder.ToTable("MemberContacts");

        builder.HasKey(contact => contact.Id);

        builder.Property(contact => contact.Landline)
            .HasMaxLength(20);

        builder.Property(contact => contact.MobileNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(contact => contact.Email)
            .HasMaxLength(256)
            .IsRequired();

        // One contact record per member.
        builder.HasIndex(contact => contact.MemberId).IsUnique();

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(contact => contact.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
