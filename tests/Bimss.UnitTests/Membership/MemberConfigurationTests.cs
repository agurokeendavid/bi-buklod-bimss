using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.UnitTests.Membership;

public class MemberConfigurationTests
{
    [Fact]
    public void Member_IsConfigured_WithExpectedTableAndConstraints()
    {
        using var dbContext = CreateDbContext();

        var entityType = dbContext.Model.FindEntityType(typeof(Member));
        Assert.NotNull(entityType);
        Assert.Equal("Members", entityType.GetTableName());

        AssertRequiredWithMaxLength(entityType, nameof(Member.LastName), 100);
        AssertRequiredWithMaxLength(entityType, nameof(Member.FirstName), 100);
        AssertRequiredWithMaxLength(entityType, nameof(Member.PlaceOfBirth), 200);

        var middleNameProperty = entityType.FindProperty(nameof(Member.MiddleName));
        Assert.NotNull(middleNameProperty);
        Assert.True(middleNameProperty.IsNullable);

        var civilStatusIdProperty = entityType.FindProperty(nameof(Member.CivilStatusId));
        Assert.NotNull(civilStatusIdProperty);
        Assert.False(civilStatusIdProperty.IsNullable);

        var suffixIdProperty = entityType.FindProperty(nameof(Member.SuffixId));
        Assert.NotNull(suffixIdProperty);
        Assert.True(suffixIdProperty.IsNullable);

        Assert.Contains(entityType.GetForeignKeys(), fk => fk.Properties.Any(p => p.Name == nameof(Member.CivilStatusId)));
        Assert.Contains(entityType.GetForeignKeys(), fk => fk.Properties.Any(p => p.Name == nameof(Member.SuffixId)));
    }

    [Fact]
    public void MemberStatusHistory_IsConfigured_WithExpectedTableAndConstraints()
    {
        using var dbContext = CreateDbContext();

        var entityType = dbContext.Model.FindEntityType(typeof(MemberStatusHistory));
        Assert.NotNull(entityType);
        Assert.Equal("MemberStatusHistory", entityType.GetTableName());

        var reasonIdProperty = entityType.FindProperty(nameof(MemberStatusHistory.ReasonId));
        Assert.NotNull(reasonIdProperty);
        Assert.True(reasonIdProperty.IsNullable);

        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(MemberStatusHistory.ReasonId)));
        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(MemberStatusHistory.MemberId)));

        Assert.DoesNotContain(
            entityType.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(MemberStatusHistory.ActorUserId)));
    }

    private static void AssertRequiredWithMaxLength(Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType, string propertyName, int maxLength)
    {
        var property = entityType.FindProperty(propertyName);
        Assert.NotNull(property);
        Assert.False(property.IsNullable);
        Assert.Equal(maxLength, property.GetMaxLength());
    }

    private static BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        return new BimssDbContext(optionsBuilder.Options);
    }
}
