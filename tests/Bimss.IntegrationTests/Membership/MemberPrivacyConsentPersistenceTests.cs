using Bimss.Domain.Membership;
using Bimss.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Membership;

public class MemberPrivacyConsentPersistenceTests
{
    private static readonly DateTimeOffset ConsentedAt = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task MemberPrivacyConsent_RoundTrips_ThroughPersistence()
    {
        var id = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var consent = new MemberPrivacyConsent(id, memberId, true, "2026-08-14", ConsentedAt, "Web Form");
            writeContext.MemberPrivacyConsents.Add(consent);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.MemberPrivacyConsents.SingleAsync();

        Assert.Equal(id, persisted.Id);
        Assert.Equal(memberId, persisted.MemberId);
        Assert.True(persisted.ConsentGiven);
        Assert.Equal("2026-08-14", persisted.NoticeVersion);
        Assert.Equal(ConsentedAt, persisted.ConsentedAtUtc);
        Assert.Equal("Web Form", persisted.Source);
    }

    [Fact]
    public async Task MemberPrivacyConsent_Accumulates_MultipleEvents_ForTheSameMember()
    {
        var memberId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.MemberPrivacyConsents.Add(
                new MemberPrivacyConsent(Guid.NewGuid(), memberId, true, "2026-08-14", ConsentedAt, "Web Form"));
            writeContext.MemberPrivacyConsents.Add(
                new MemberPrivacyConsent(Guid.NewGuid(), memberId, true, "2027-01-01", ConsentedAt.AddMonths(5), "Web Form"));
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var consents = await readContext.MemberPrivacyConsents.Where(c => c.MemberId == memberId).ToListAsync();

        Assert.Equal(2, consents.Count);
        Assert.Contains(consents, c => c.NoticeVersion == "2026-08-14");
        Assert.Contains(consents, c => c.NoticeVersion == "2027-01-01");
    }
}
