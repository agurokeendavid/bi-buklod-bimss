using Bimss.Domain.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Membership;

public class MemberChildConfiguration : IEntityTypeConfiguration<MemberChild>
{
    public void Configure(EntityTypeBuilder<MemberChild> builder)
    {
        builder.ToTable("MemberChildren");

        builder.HasKey(child => child.Id);

        builder.Property(child => child.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(child => child.DateOfBirth)
            .IsRequired();

        // A member can have more than one child, so MemberId is indexed for
        // lookups but not unique.
        builder.HasIndex(child => child.MemberId);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(child => child.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
