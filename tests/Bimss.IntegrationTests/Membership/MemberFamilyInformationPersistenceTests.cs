using Bimss.Domain.Membership;
using Bimss.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Membership;

public class MemberFamilyInformationPersistenceTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task MemberFamilyInformation_RoundTrips_ThroughPersistence()
    {
        var id = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var family = new MemberFamilyInformation(
                id, memberId, "Maria Dela Cruz", "Pedro Dela Cruz", "Reyes", "12 Mabini St., Batangas");
            writeContext.MemberFamilyInformation.Add(family);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.MemberFamilyInformation.SingleAsync();

        Assert.Equal(id, persisted.Id);
        Assert.Equal(memberId, persisted.MemberId);
        Assert.Equal("Maria Dela Cruz", persisted.SpouseFullName);
        Assert.Equal("Pedro Dela Cruz", persisted.FatherFullName);
        Assert.Equal("Reyes", persisted.MotherMaidenName);
        Assert.Equal("12 Mabini St., Batangas", persisted.ParentsPresentAddress);
    }

    [Fact]
    public async Task UpdateDetails_Persists_AcrossReloads()
    {
        var memberId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var family = new MemberFamilyInformation(Guid.NewGuid(), memberId, null, null, null, null);
            writeContext.MemberFamilyInformation.Add(family);
            await writeContext.SaveChangesAsync();
        }

        await using (var updateContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var family = await updateContext.MemberFamilyInformation.SingleAsync();
            family.UpdateDetails("Maria Dela Cruz", "Pedro Dela Cruz", "Reyes", "12 Mabini St., Batangas");
            await updateContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.MemberFamilyInformation.SingleAsync();

        Assert.Equal("Maria Dela Cruz", persisted.SpouseFullName);
        Assert.Equal("Pedro Dela Cruz", persisted.FatherFullName);
        Assert.Equal("Reyes", persisted.MotherMaidenName);
        Assert.Equal("12 Mabini St., Batangas", persisted.ParentsPresentAddress);
    }
}
