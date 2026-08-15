using Bimss.Application.Membership;
using Bimss.Domain.Membership;
using Bimss.IntegrationTests.Support;
using Bimss.Infrastructure.Membership;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Membership;

public class MemberStatusTransitionPersistenceTests
{
    private static readonly DateTimeOffset OccurredAt = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetTrackedByIdAsync_ReturnsNull_WhenMemberDoesNotExist()
    {
        await using var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        IMemberRepository repository = new MemberRepository(dbContext);

        var member = await repository.GetTrackedByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(member);
    }

    [Fact]
    public async Task VerifyThenSaveChanges_PersistsStatusAndHistory_AcrossReloads()
    {
        var memberId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.Members.Add(new Member(
                memberId, "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
                Guid.NewGuid(), joiningReason: null, OccurredAt));
            await writeContext.SaveChangesAsync();
        }

        var actorUserId = Guid.NewGuid();
        await using (var verifyContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            IMemberRepository repository = new MemberRepository(verifyContext);
            var member = await repository.GetTrackedByIdAsync(memberId, CancellationToken.None);
            Assert.NotNull(member);

            member!.Verify(actorUserId, OccurredAt, "Documents checked");
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.Members.Include(m => m.StatusHistory).SingleAsync();

        Assert.Equal(MemberStatus.Active, persisted.Status);
        Assert.Equal(2, persisted.StatusHistory.Count);
        Assert.Contains(persisted.StatusHistory, h => h.ToStatus == MemberStatus.Active && h.ActorUserId == actorUserId);
    }
}
