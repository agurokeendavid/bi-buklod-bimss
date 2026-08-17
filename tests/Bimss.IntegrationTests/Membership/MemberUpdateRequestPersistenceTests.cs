using Bimss.Domain.Membership;
using Bimss.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Membership;

public class MemberUpdateRequestPersistenceTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();
    private static readonly DateTimeOffset OccurredAt = new(2026, 8, 17, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task MemberUpdateRequest_RoundTrips_WithItsChanges()
    {
        var memberId = Guid.NewGuid();
        var submittedByUserId = Guid.NewGuid();
        var id = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var request = new MemberUpdateRequest(
                id,
                memberId,
                submittedByUserId,
                OccurredAt,
                [
                    new MemberUpdateRequestChangeInput("LastName", "Dela Cruz", "Santos"),
                    new MemberUpdateRequestChangeInput("CivilStatusId", null, Guid.NewGuid().ToString()),
                ]);
            writeContext.MemberUpdateRequests.Add(request);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.MemberUpdateRequests.SingleAsync();
        var changes = await readContext.MemberUpdateRequestChanges
            .Where(change => change.MemberUpdateRequestId == id)
            .ToListAsync();

        Assert.Equal(id, persisted.Id);
        Assert.Equal(memberId, persisted.MemberId);
        Assert.Equal(MemberUpdateRequestStatus.Pending, persisted.Status);
        Assert.Equal(2, changes.Count);
        Assert.Contains(changes, change => change.FieldName == "LastName" && change.NewValue == "Santos");
    }

    [Fact]
    public async Task Approve_Persists_AcrossReloads()
    {
        var id = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var request = new MemberUpdateRequest(
                id, Guid.NewGuid(), Guid.NewGuid(), OccurredAt, [new MemberUpdateRequestChangeInput("LastName", "Dela Cruz", "Santos")]);
            writeContext.MemberUpdateRequests.Add(request);
            await writeContext.SaveChangesAsync();
        }

        var reviewerId = Guid.NewGuid();
        await using (var reviewContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var request = await reviewContext.MemberUpdateRequests.SingleAsync();
            request.Approve(reviewerId, OccurredAt, "Confirmed with member");
            await reviewContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.MemberUpdateRequests.SingleAsync();

        Assert.Equal(MemberUpdateRequestStatus.Approved, persisted.Status);
        Assert.Equal(reviewerId, persisted.ReviewedByUserId);
        Assert.Equal("Confirmed with member", persisted.ReviewRemarks);
    }
}
