using Bimss.Domain.Membership;
using Bimss.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Membership;

public class MemberEducationPersistenceTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task MemberEducation_RoundTrips_ThroughPersistence()
    {
        var id = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var attainmentId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var education = new MemberEducation(id, memberId, attainmentId, "BS Criminology");
            writeContext.MemberEducations.Add(education);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.MemberEducations.SingleAsync();

        Assert.Equal(id, persisted.Id);
        Assert.Equal(memberId, persisted.MemberId);
        Assert.Equal(attainmentId, persisted.HighestAttainmentId);
        Assert.Equal("BS Criminology", persisted.DegreeCourse);
    }

    [Fact]
    public async Task UpdateDetails_Persists_AcrossReloads()
    {
        var memberId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var education = new MemberEducation(Guid.NewGuid(), memberId, Guid.NewGuid(), "BS Criminology");
            writeContext.MemberEducations.Add(education);
            await writeContext.SaveChangesAsync();
        }

        var newAttainmentId = Guid.NewGuid();
        await using (var updateContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var education = await updateContext.MemberEducations.SingleAsync();
            education.UpdateDetails(newAttainmentId, "MA Public Administration");
            await updateContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.MemberEducations.SingleAsync();

        Assert.Equal(newAttainmentId, persisted.HighestAttainmentId);
        Assert.Equal("MA Public Administration", persisted.DegreeCourse);
    }
}
