using Bimss.Domain.Membership;
using Bimss.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Membership;

public class MemberAddressPersistenceTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task MemberAddress_RoundTrips_BothTypes_ForTheSameMember()
    {
        var memberId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.MemberAddresses.Add(
                new MemberAddress(Guid.NewGuid(), memberId, MemberAddressType.Present, "123 Rizal St., Manila"));
            writeContext.MemberAddresses.Add(
                new MemberAddress(Guid.NewGuid(), memberId, MemberAddressType.Permanent, "45 Mabini Ave., Batangas"));
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var addresses = await readContext.MemberAddresses.Where(a => a.MemberId == memberId).ToListAsync();

        Assert.Equal(2, addresses.Count);
        Assert.Contains(addresses, a => a.AddressType == MemberAddressType.Present && a.AddressLine == "123 Rizal St., Manila");
        Assert.Contains(addresses, a => a.AddressType == MemberAddressType.Permanent && a.AddressLine == "45 Mabini Ave., Batangas");
    }

    [Fact]
    public async Task UpdateAddressLine_Persists_AcrossReloads()
    {
        var memberId = Guid.NewGuid();
        var id = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.MemberAddresses.Add(new MemberAddress(id, memberId, MemberAddressType.Present, "Old address"));
            await writeContext.SaveChangesAsync();
        }

        await using (var updateContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var address = await updateContext.MemberAddresses.SingleAsync();
            address.UpdateAddressLine("New address");
            await updateContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.MemberAddresses.SingleAsync();

        Assert.Equal("New address", persisted.AddressLine);
    }
}
