using Bimss.Domain.Membership;
using Bimss.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Membership;

public class MemberChildPersistenceTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task MemberChild_RoundTrips_MultipleRows_ForTheSameMember()
    {
        var memberId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.MemberChildren.Add(new MemberChild(Guid.NewGuid(), memberId, "Maria Dela Cruz", new DateOnly(2015, 4, 10)));
            writeContext.MemberChildren.Add(new MemberChild(Guid.NewGuid(), memberId, "Juan Dela Cruz Jr.", new DateOnly(2018, 9, 2)));
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var children = await readContext.MemberChildren.Where(c => c.MemberId == memberId).ToListAsync();

        Assert.Equal(2, children.Count);
        Assert.Contains(children, c => c.Name == "Maria Dela Cruz" && c.DateOfBirth == new DateOnly(2015, 4, 10));
        Assert.Contains(children, c => c.Name == "Juan Dela Cruz Jr." && c.DateOfBirth == new DateOnly(2018, 9, 2));
    }

    [Fact]
    public async Task UpdateDetails_Persists_AcrossReloads()
    {
        var memberId = Guid.NewGuid();
        var id = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.MemberChildren.Add(new MemberChild(id, memberId, "Maria Dela Cruz", new DateOnly(2015, 4, 10)));
            await writeContext.SaveChangesAsync();
        }

        await using (var updateContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var child = await updateContext.MemberChildren.SingleAsync();
            child.UpdateDetails("Maria D. Cruz", new DateOnly(2015, 4, 11));
            await updateContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.MemberChildren.SingleAsync();

        Assert.Equal("Maria D. Cruz", persisted.Name);
        Assert.Equal(new DateOnly(2015, 4, 11), persisted.DateOfBirth);
    }
}
