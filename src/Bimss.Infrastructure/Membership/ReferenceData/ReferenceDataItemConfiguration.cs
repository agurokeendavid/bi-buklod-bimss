using Bimss.Domain.Membership.ReferenceData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Membership.ReferenceData;

public abstract class ReferenceDataItemConfiguration<T> : IEntityTypeConfiguration<T>
    where T : ReferenceDataItem
{
    protected abstract string TableName { get; }

    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(item => item.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(item => item.Code)
            .IsUnique();
    }
}
