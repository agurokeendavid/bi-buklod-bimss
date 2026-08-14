using Bimss.Domain.Membership.ReferenceData;
using Bimss.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Membership.ReferenceData;

public class ReferenceDataPersistenceTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task CivilStatus_RoundTrips_ThroughPersistence()
    {
        var id = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.CivilStatuses.Add(new CivilStatus(id, "MARRIED", "Married"));
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.CivilStatuses.SingleAsync();

        Assert.Equal(id, persisted.Id);
        Assert.Equal("MARRIED", persisted.Code);
        Assert.Equal("Married", persisted.Name);
        Assert.True(persisted.IsActive);
    }

    [Fact]
    public async Task SetActive_PersistsAsInactive_AfterReload()
    {
        var id = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.OfficeUnits.Add(new OfficeUnit(id, "HR", "Human Resources"));
            await writeContext.SaveChangesAsync();
        }

        await using (var deactivateContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var officeUnit = await deactivateContext.OfficeUnits.SingleAsync();
            officeUnit.SetActive(false);
            await deactivateContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.OfficeUnits.SingleAsync();

        Assert.False(persisted.IsActive);
    }
}
