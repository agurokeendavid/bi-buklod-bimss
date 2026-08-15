using Bimss.Domain.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Membership;

public class MemberFamilyInformationConfiguration : IEntityTypeConfiguration<MemberFamilyInformation>
{
    public void Configure(EntityTypeBuilder<MemberFamilyInformation> builder)
    {
        builder.ToTable("MemberFamilyInformation");

        builder.HasKey(family => family.Id);

        builder.Property(family => family.SpouseFullName)
            .HasMaxLength(200);

        builder.Property(family => family.FatherFullName)
            .HasMaxLength(200);

        builder.Property(family => family.MotherMaidenName)
            .HasMaxLength(200);

        builder.Property(family => family.ParentsPresentAddress)
            .HasMaxLength(500);

        // One family information record per member.
        builder.HasIndex(family => family.MemberId).IsUnique();

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(family => family.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
