using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.UnitTests.Membership;

public class MemberPrivacyConsentConfigurationTests
{
    [Fact]
    public void MemberPrivacyConsent_IsConfigured_WithExpectedTableAndConstraints()
    {
        using var dbContext = CreateDbContext();

        var entityType = dbContext.Model.FindEntityType(typeof(MemberPrivacyConsent));
        Assert.NotNull(entityType);
        Assert.Equal("MemberPrivacyConsents", entityType.GetTableName());

        var noticeVersionProperty = entityType.FindProperty(nameof(MemberPrivacyConsent.NoticeVersion));
        Assert.NotNull(noticeVersionProperty);
        Assert.False(noticeVersionProperty.IsNullable);
        Assert.Equal(50, noticeVersionProperty.GetMaxLength());

        var sourceProperty = entityType.FindProperty(nameof(MemberPrivacyConsent.Source));
        Assert.NotNull(sourceProperty);
        Assert.False(sourceProperty.IsNullable);
        Assert.Equal(100, sourceProperty.GetMaxLength());

        var consentGivenProperty = entityType.FindProperty(nameof(MemberPrivacyConsent.ConsentGiven));
        Assert.NotNull(consentGivenProperty);
        Assert.False(consentGivenProperty.IsNullable);

        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(MemberPrivacyConsent.MemberId)));

        // A member accumulates one row per consent event, so MemberId must not be unique.
        Assert.DoesNotContain(
            entityType.GetIndexes(),
            index => index.IsUnique && index.Properties.Any(p => p.Name == nameof(MemberPrivacyConsent.MemberId)));
    }

    private static BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        return new BimssDbContext(optionsBuilder.Options);
    }
}
