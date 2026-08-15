using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.UnitTests.Membership;

public class MemberFamilyInformationConfigurationTests
{
    [Fact]
    public void MemberFamilyInformation_IsConfigured_WithExpectedTableAndConstraints()
    {
        using var dbContext = CreateDbContext();

        var entityType = dbContext.Model.FindEntityType(typeof(MemberFamilyInformation));
        Assert.NotNull(entityType);
        Assert.Equal("MemberFamilyInformation", entityType.GetTableName());

        var spouseProperty = entityType.FindProperty(nameof(MemberFamilyInformation.SpouseFullName));
        Assert.NotNull(spouseProperty);
        Assert.True(spouseProperty.IsNullable);
        Assert.Equal(200, spouseProperty.GetMaxLength());

        var addressProperty = entityType.FindProperty(nameof(MemberFamilyInformation.ParentsPresentAddress));
        Assert.NotNull(addressProperty);
        Assert.True(addressProperty.IsNullable);
        Assert.Equal(500, addressProperty.GetMaxLength());

        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(MemberFamilyInformation.MemberId)));

        Assert.Contains(
            entityType.GetIndexes(),
            index => index.IsUnique && index.Properties.Any(p => p.Name == nameof(MemberFamilyInformation.MemberId)));
    }

    private static BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        return new BimssDbContext(optionsBuilder.Options);
    }
}
