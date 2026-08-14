using Bimss.Domain.Membership;
using Bimss.Domain.Membership.ReferenceData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Membership;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");

        builder.HasKey(member => member.Id);

        builder.Property(member => member.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(member => member.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(member => member.MiddleName)
            .HasMaxLength(100);

        builder.Property(member => member.PlaceOfBirth)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(member => member.JoiningReason)
            .HasMaxLength(2000);

        builder.Property(member => member.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasOne<Suffix>()
            .WithMany()
            .HasForeignKey(member => member.SuffixId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CivilStatus>()
            .WithMany()
            .HasForeignKey(member => member.CivilStatusId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.Navigation(member => member.StatusHistory)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(member => member.StatusHistory)
            .WithOne()
            .HasForeignKey(history => history.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
