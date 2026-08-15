using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.UnitTests.Membership;

public class MemberAddressConfigurationTests
{
    [Fact]
    public void MemberAddress_IsConfigured_WithExpectedTableAndConstraints()
    {
        using var dbContext = CreateDbContext();

        var entityType = dbContext.Model.FindEntityType(typeof(MemberAddress));
        Assert.NotNull(entityType);
        Assert.Equal("MemberAddresses", entityType.GetTableName());

        var addressLineProperty = entityType.FindProperty(nameof(MemberAddress.AddressLine));
        Assert.NotNull(addressLineProperty);
        Assert.False(addressLineProperty.IsNullable);
        Assert.Equal(500, addressLineProperty.GetMaxLength());

        var addressTypeProperty = entityType.FindProperty(nameof(MemberAddress.AddressType));
        Assert.NotNull(addressTypeProperty);
        Assert.False(addressTypeProperty.IsNullable);

        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(MemberAddress.MemberId)));

        Assert.Contains(
            entityType.GetIndexes(),
            index => index.IsUnique
                && index.Properties.Any(p => p.Name == nameof(MemberAddress.MemberId))
                && index.Properties.Any(p => p.Name == nameof(MemberAddress.AddressType)));
    }

    private static BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        return new BimssDbContext(optionsBuilder.Options);
    }
}
