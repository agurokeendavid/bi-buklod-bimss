using Bimss.Application.Auditing;
using Bimss.Application.Membership;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberStatusTransitionServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task VerifyAsync_Succeeds_AndLogsAudit()
    {
        var member = CreateMember();
        var repository = new FakeMemberRepository { Member = member };
        var auditLogger = new FakeAuditLogger();
        var service = new MemberStatusTransitionService(repository, auditLogger, new FixedTimeProvider(Now));
        var actorUserId = Guid.NewGuid();

        await service.VerifyAsync(member.Id, actorUserId, "Documents checked");

        Assert.Equal(MemberStatus.Active, member.Status);
        Assert.True(repository.SaveChangesCalled);
        Assert.NotNull(auditLogger.LoggedEntry);
        Assert.Equal("Member.Verify", auditLogger.LoggedEntry!.Action);
        Assert.Equal(actorUserId, auditLogger.LoggedEntry.ActorUserId);
        Assert.Equal(member.Id.ToString(), auditLogger.LoggedEntry.ObjectId);
    }

    [Fact]
    public async Task VerifyAsync_Throws_NotFound_WhenMemberDoesNotExist()
    {
        var repository = new FakeMemberRepository { Member = null };
        var auditLogger = new FakeAuditLogger();
        var service = new MemberStatusTransitionService(repository, auditLogger, new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<NotFoundException>(() => service.VerifyAsync(Guid.NewGuid(), Guid.NewGuid(), null));

        Assert.False(repository.SaveChangesCalled);
        Assert.Null(auditLogger.LoggedEntry);
    }

    [Fact]
    public async Task VerifyAsync_Throws_Conflict_WhenNotPendingVerification_AndDoesNotLogAudit()
    {
        var member = CreateMember();
        member.Verify(Guid.NewGuid(), Now);
        var repository = new FakeMemberRepository { Member = member };
        var auditLogger = new FakeAuditLogger();
        var service = new MemberStatusTransitionService(repository, auditLogger, new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<ConflictException>(() => service.VerifyAsync(member.Id, Guid.NewGuid(), null));

        Assert.False(repository.SaveChangesCalled);
        Assert.Null(auditLogger.LoggedEntry);
    }

    [Fact]
    public async Task DeactivateAsync_Succeeds_AndLogsAudit()
    {
        var member = CreateMember();
        member.Verify(Guid.NewGuid(), Now);
        var repository = new FakeMemberRepository { Member = member };
        var auditLogger = new FakeAuditLogger();
        var service = new MemberStatusTransitionService(repository, auditLogger, new FixedTimeProvider(Now));
        var reasonId = Guid.NewGuid();

        await service.DeactivateAsync(member.Id, reasonId, Guid.NewGuid(), "Resigned from BI");

        Assert.Equal(MemberStatus.Inactive, member.Status);
        Assert.True(repository.SaveChangesCalled);
        Assert.NotNull(auditLogger.LoggedEntry);
        Assert.Equal("Member.Deactivate", auditLogger.LoggedEntry!.Action);
    }

    [Fact]
    public async Task DeactivateAsync_Throws_NotFound_WhenMemberDoesNotExist()
    {
        var repository = new FakeMemberRepository { Member = null };
        var service = new MemberStatusTransitionService(repository, new FakeAuditLogger(), new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeactivateAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null));
    }

    [Fact]
    public async Task ReactivateAsync_Succeeds_AndLogsAudit()
    {
        var member = CreateMember();
        member.Verify(Guid.NewGuid(), Now);
        member.Deactivate(Guid.NewGuid(), Guid.NewGuid(), Now);
        var repository = new FakeMemberRepository { Member = member };
        var auditLogger = new FakeAuditLogger();
        var service = new MemberStatusTransitionService(repository, auditLogger, new FixedTimeProvider(Now));

        await service.ReactivateAsync(member.Id, Guid.NewGuid(), "Rejoined");

        Assert.Equal(MemberStatus.Active, member.Status);
        Assert.True(repository.SaveChangesCalled);
        Assert.NotNull(auditLogger.LoggedEntry);
        Assert.Equal("Member.Reactivate", auditLogger.LoggedEntry!.Action);
    }

    [Fact]
    public async Task ReactivateAsync_Throws_NotFound_WhenMemberDoesNotExist()
    {
        var repository = new FakeMemberRepository { Member = null };
        var service = new MemberStatusTransitionService(repository, new FakeAuditLogger(), new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<NotFoundException>(() => service.ReactivateAsync(Guid.NewGuid(), Guid.NewGuid(), null));
    }

    private static Member CreateMember()
    {
        return new Member(
            Guid.NewGuid(), "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
            Guid.NewGuid(), joiningReason: null, Now);
    }

    private sealed class FakeMemberRepository : IMemberRepository
    {
        public Member? Member { get; set; }

        public bool SaveChangesCalled { get; private set; }

        public Task<bool> EmployeeNumberExistsAsync(string employeeNumber, CancellationToken cancellationToken)
            => Task.FromResult(false);

        public Task AddAsync(Member member, MemberEmployment employment, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<Member?> GetTrackedByIdAsync(Guid memberId, CancellationToken cancellationToken)
            => Task.FromResult(Member);

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAuditLogger : IAuditLogger
    {
        public AuditEntry? LoggedEntry { get; private set; }

        public Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default)
        {
            LoggedEntry = entry;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
