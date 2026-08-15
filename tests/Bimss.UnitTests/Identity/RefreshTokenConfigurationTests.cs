using Bimss.Infrastructure.Identity;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.UnitTests.Identity;

public class RefreshTokenConfigurationTests
{
    [Fact]
    public void RefreshToken_IsConfigured_WithExpectedTableAndConstraints()
    {
        using var dbContext = CreateDbContext();

        var entityType = dbContext.Model.FindEntityType(typeof(RefreshToken));
        Assert.NotNull(entityType);
        Assert.Equal("RefreshTokens", entityType.GetTableName());

        var tokenHashProperty = entityType.FindProperty(nameof(RefreshToken.TokenHash));
        Assert.NotNull(tokenHashProperty);
        Assert.False(tokenHashProperty.IsNullable);
        Assert.Equal(200, tokenHashProperty.GetMaxLength());

        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(RefreshToken.UserId)));

        Assert.Contains(
            entityType.GetIndexes(),
            index => index.IsUnique && index.Properties.Any(p => p.Name == nameof(RefreshToken.TokenHash)));
    }

    private static BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        return new BimssDbContext(optionsBuilder.Options);
    }
}
