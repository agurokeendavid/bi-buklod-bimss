using Bimss.Domain.Membership;
using Bimss.Domain.Membership.ReferenceData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Membership;

public class MemberEducationConfiguration : IEntityTypeConfiguration<MemberEducation>
{
    public void Configure(EntityTypeBuilder<MemberEducation> builder)
    {
        builder.ToTable("MemberEducations");

        builder.HasKey(education => education.Id);

        builder.Property(education => education.DegreeCourse)
            .HasMaxLength(200);

        // One education record per member.
        builder.HasIndex(education => education.MemberId).IsUnique();

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(education => education.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<EducationalAttainment>()
            .WithMany()
            .HasForeignKey(education => education.HighestAttainmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
