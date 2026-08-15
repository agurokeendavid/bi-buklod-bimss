using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.UnitTests.Membership;

public class MemberContactConfigurationTests
{
    [Fact]
    public void MemberContact_IsConfigured_WithExpectedTableAndConstraints()
    {
        using var dbContext = CreateDbContext();

        var entityType = dbContext.Model.FindEntityType(typeof(MemberContact));
        Assert.NotNull(entityType);
        Assert.Equal("MemberContacts", entityType.GetTableName());

        var mobileProperty = entityType.FindProperty(nameof(MemberContact.MobileNumber));
        Assert.NotNull(mobileProperty);
        Assert.False(mobileProperty.IsNullable);
        Assert.Equal(20, mobileProperty.GetMaxLength());

        var emailProperty = entityType.FindProperty(nameof(MemberContact.Email));
        Assert.NotNull(emailProperty);
        Assert.False(emailProperty.IsNullable);
        Assert.Equal(256, emailProperty.GetMaxLength());

        var landlineProperty = entityType.FindProperty(nameof(MemberContact.Landline));
        Assert.NotNull(landlineProperty);
        Assert.True(landlineProperty.IsNullable);

        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(MemberContact.MemberId)));

        Assert.Contains(
            entityType.GetIndexes(),
            index => index.IsUnique && index.Properties.Any(p => p.Name == nameof(MemberContact.MemberId)));
    }

    private static BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        return new BimssDbContext(optionsBuilder.Options);
    }
}
