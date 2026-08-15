using Bimss.Domain.Membership;
using Bimss.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Membership;

public class MemberEmploymentPersistenceTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task MemberEmployment_RoundTrips_ThroughPersistence()
    {
        var memberId = Guid.NewGuid();
        var officeUnitId = Guid.NewGuid();
        var id = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var employment = new MemberEmployment(
                id, memberId, "BI-00123", "Immigration Officer I", officeUnitId, new DateOnly(2020, 6, 1));
            writeContext.MemberEmployments.Add(employment);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.MemberEmployments.SingleAsync();

        Assert.Equal(id, persisted.Id);
        Assert.Equal(memberId, persisted.MemberId);
        Assert.Equal("BI-00123", persisted.EmployeeNumber);
        Assert.Equal("Immigration Officer I", persisted.PositionDesignation);
        Assert.Equal(officeUnitId, persisted.OfficeUnitId);
        Assert.Equal(new DateOnly(2020, 6, 1), persisted.PermanentAppointmentDate);
    }

    [Fact]
    public async Task UpdateDetails_Persists_AcrossReloads()
    {
        var memberId = Guid.NewGuid();
        var id = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var employment = new MemberEmployment(
                id, memberId, "BI-00123", "Immigration Officer I", Guid.NewGuid(), permanentAppointmentDate: null);
            writeContext.MemberEmployments.Add(employment);
            await writeContext.SaveChangesAsync();
        }

        var newOfficeUnitId = Guid.NewGuid();
        await using (var updateContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var employment = await updateContext.MemberEmployments.SingleAsync();
            employment.UpdateDetails("Immigration Officer II", newOfficeUnitId, new DateOnly(2022, 1, 10));
            await updateContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.MemberEmployments.SingleAsync();

        Assert.Equal("Immigration Officer II", persisted.PositionDesignation);
        Assert.Equal(newOfficeUnitId, persisted.OfficeUnitId);
        Assert.Equal(new DateOnly(2022, 1, 10), persisted.PermanentAppointmentDate);
    }
}
