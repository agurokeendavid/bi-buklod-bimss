using Bimss.Domain.Membership;
using Bimss.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Membership;

public class MemberPersistenceTests
{
    private static readonly DateTimeOffset OccurredAt = new(2026, 8, 14, 0, 0, 0, TimeSpan.Zero);

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task Member_RoundTrips_ThroughPersistence_WithInitialStatusHistory()
    {
        var id = Guid.NewGuid();
        var civilStatusId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var member = new Member(
                id, "Dela Cruz", "Juan", "Santos", suffixId: null, new DateOnly(1990, 1, 1), "Manila", civilStatusId, "Referred by a colleague", OccurredAt);
            writeContext.Members.Add(member);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.Members
            .Include(member => member.StatusHistory)
            .SingleAsync();

        Assert.Equal(id, persisted.Id);
        Assert.Equal("Dela Cruz", persisted.LastName);
        Assert.Equal(civilStatusId, persisted.CivilStatusId);
        Assert.Equal(MemberStatus.PendingVerification, persisted.Status);

        var historyEntry = Assert.Single(persisted.StatusHistory);
        Assert.Null(historyEntry.FromStatus);
        Assert.Equal(MemberStatus.PendingVerification, historyEntry.ToStatus);
    }

    [Fact]
    public async Task StatusTransitions_Accumulate_InOrder_AcrossReloads()
    {
        var id = Guid.NewGuid();
        var verifyActorId = Guid.NewGuid();
        var deactivateActorId = Guid.NewGuid();
        var reasonId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var member = new Member(
                id, "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila", Guid.NewGuid(), joiningReason: null, OccurredAt);
            writeContext.Members.Add(member);
            await writeContext.SaveChangesAsync();
        }

        await using (var verifyContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var member = await verifyContext.Members.Include(m => m.StatusHistory).SingleAsync();
            member.Verify(verifyActorId, OccurredAt);
            await verifyContext.SaveChangesAsync();
        }

        await using (var deactivateContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var member = await deactivateContext.Members.Include(m => m.StatusHistory).SingleAsync();
            member.Deactivate(deactivateActorId, reasonId, OccurredAt, "Resigned from BI");
            await deactivateContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.Members.Include(m => m.StatusHistory).SingleAsync();

        Assert.Equal(MemberStatus.Inactive, persisted.Status);
        Assert.Equal(3, persisted.StatusHistory.Count);

        var orderedHistory = persisted.StatusHistory.OrderBy(h => h.OccurredAtUtc).ThenBy(h => h.ToStatus).ToList();
        var deactivateEntry = Assert.Single(orderedHistory, h => h.ToStatus == MemberStatus.Inactive);
        Assert.Equal(reasonId, deactivateEntry.ReasonId);
        Assert.Equal(deactivateActorId, deactivateEntry.ActorUserId);
    }
}
