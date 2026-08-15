using Bimss.Domain.Membership;
using Bimss.Domain.Membership.ReferenceData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Membership;

public class MemberEmploymentConfiguration : IEntityTypeConfiguration<MemberEmployment>
{
    public void Configure(EntityTypeBuilder<MemberEmployment> builder)
    {
        builder.ToTable("MemberEmployments");

        builder.HasKey(employment => employment.Id);

        builder.Property(employment => employment.EmployeeNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(employment => employment.PositionDesignation)
            .HasMaxLength(200)
            .IsRequired();

        // BI Employee Number is unique and mandatory (confirmed with Buklod, 2026-08-14).
        builder.HasIndex(employment => employment.EmployeeNumber).IsUnique();

        // One employment record per member.
        builder.HasIndex(employment => employment.MemberId).IsUnique();

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(employment => employment.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<OfficeUnit>()
            .WithMany()
            .HasForeignKey(employment => employment.OfficeUnitId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
