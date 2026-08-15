using Bimss.Domain.Membership;
using Bimss.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Membership;

public class MemberEligibilityPersistenceTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task MemberEligibility_RoundTrips_MultipleRows_ForTheSameMember()
    {
        var memberId = Guid.NewGuid();
        var civilServiceTypeId = Guid.NewGuid();
        var prcTypeId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.MemberEligibilities.Add(
                new MemberEligibility(Guid.NewGuid(), memberId, civilServiceTypeId, "Civil Service Professional"));
            writeContext.MemberEligibilities.Add(
                new MemberEligibility(Guid.NewGuid(), memberId, prcTypeId, "PRC License No. 0123456"));
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var eligibilities = await readContext.MemberEligibilities.Where(e => e.MemberId == memberId).ToListAsync();

        Assert.Equal(2, eligibilities.Count);
        Assert.Contains(eligibilities, e => e.EligibilityTypeId == civilServiceTypeId && e.Details == "Civil Service Professional");
        Assert.Contains(eligibilities, e => e.EligibilityTypeId == prcTypeId && e.Details == "PRC License No. 0123456");
    }

    [Fact]
    public async Task UpdateDetails_Persists_AcrossReloads()
    {
        var memberId = Guid.NewGuid();
        var id = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.MemberEligibilities.Add(new MemberEligibility(id, memberId, Guid.NewGuid(), "Old details"));
            await writeContext.SaveChangesAsync();
        }

        await using (var updateContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var eligibility = await updateContext.MemberEligibilities.SingleAsync();
            eligibility.UpdateDetails("New details");
            await updateContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.MemberEligibilities.SingleAsync();

        Assert.Equal("New details", persisted.Details);
    }
}
