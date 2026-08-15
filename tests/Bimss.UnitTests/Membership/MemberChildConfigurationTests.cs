using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.UnitTests.Membership;

public class MemberChildConfigurationTests
{
    [Fact]
    public void MemberChild_IsConfigured_WithExpectedTableAndConstraints()
    {
        using var dbContext = CreateDbContext();

        var entityType = dbContext.Model.FindEntityType(typeof(MemberChild));
        Assert.NotNull(entityType);
        Assert.Equal("MemberChildren", entityType.GetTableName());

        var nameProperty = entityType.FindProperty(nameof(MemberChild.Name));
        Assert.NotNull(nameProperty);
        Assert.False(nameProperty.IsNullable);
        Assert.Equal(200, nameProperty.GetMaxLength());

        var dateOfBirthProperty = entityType.FindProperty(nameof(MemberChild.DateOfBirth));
        Assert.NotNull(dateOfBirthProperty);
        Assert.False(dateOfBirthProperty.IsNullable);

        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(MemberChild.MemberId)));

        // A member can have more than one child, so MemberId must not be unique.
        Assert.DoesNotContain(
            entityType.GetIndexes(),
            index => index.IsUnique && index.Properties.Any(p => p.Name == nameof(MemberChild.MemberId)));
    }

    private static BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        return new BimssDbContext(optionsBuilder.Options);
    }
}
