using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.UnitTests.Membership;

public class MemberEligibilityConfigurationTests
{
    [Fact]
    public void MemberEligibility_IsConfigured_WithExpectedTableAndConstraints()
    {
        using var dbContext = CreateDbContext();

        var entityType = dbContext.Model.FindEntityType(typeof(MemberEligibility));
        Assert.NotNull(entityType);
        Assert.Equal("MemberEligibilities", entityType.GetTableName());

        var detailsProperty = entityType.FindProperty(nameof(MemberEligibility.Details));
        Assert.NotNull(detailsProperty);
        Assert.True(detailsProperty.IsNullable);
        Assert.Equal(500, detailsProperty.GetMaxLength());

        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(MemberEligibility.MemberId)));
        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(MemberEligibility.EligibilityTypeId)));

        // Multiple eligibilities per member are allowed, so MemberId must not be unique.
        Assert.DoesNotContain(
            entityType.GetIndexes(),
            index => index.IsUnique && index.Properties.Any(p => p.Name == nameof(MemberEligibility.MemberId)));
    }

    private static BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        return new BimssDbContext(optionsBuilder.Options);
    }
}
