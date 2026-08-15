using Bimss.Application.Auditing;
using Bimss.Application.Membership;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberProfileUpdateServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task UpdateAsync_Succeeds_AndLogsAudit()
    {
        var member = CreateMember();
        var employment = CreateEmployment(member.Id);
        var repository = new FakeMemberRepository { Member = member, Employment = employment };
        var auditLogger = new FakeAuditLogger();
        var service = new MemberProfileUpdateService(repository, auditLogger);
        var actorUserId = Guid.NewGuid();
        var command = CreateCommand();

        await service.UpdateAsync(member.Id, command, actorUserId);

        Assert.Equal(command.LastName, member.LastName);
        Assert.Equal(command.FirstName, member.FirstName);
        Assert.Equal(command.PositionDesignation, employment.PositionDesignation);
        Assert.Equal(command.OfficeUnitId, employment.OfficeUnitId);
        Assert.True(repository.SaveChangesCalled);
        Assert.NotNull(auditLogger.LoggedEntry);
        Assert.Equal("Member.UpdateProfile", auditLogger.LoggedEntry!.Action);
        Assert.Equal(actorUserId, auditLogger.LoggedEntry.ActorUserId);
        Assert.Equal(member.Id.ToString(), auditLogger.LoggedEntry.ObjectId);
    }

    [Fact]
    public async Task UpdateAsync_Throws_NotFound_WhenMemberDoesNotExist()
    {
        var repository = new FakeMemberRepository { Member = null, Employment = null };
        var auditLogger = new FakeAuditLogger();
        var service = new MemberProfileUpdateService(repository, auditLogger);

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateAsync(Guid.NewGuid(), CreateCommand(), Guid.NewGuid()));

        Assert.False(repository.SaveChangesCalled);
        Assert.Null(auditLogger.LoggedEntry);
    }

    [Fact]
    public async Task UpdateAsync_Throws_NotFound_WhenEmploymentDoesNotExist()
    {
        var member = CreateMember();
        var repository = new FakeMemberRepository { Member = member, Employment = null };
        var auditLogger = new FakeAuditLogger();
        var service = new MemberProfileUpdateService(repository, auditLogger);

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateAsync(member.Id, CreateCommand(), Guid.NewGuid()));

        Assert.False(repository.SaveChangesCalled);
        Assert.Null(auditLogger.LoggedEntry);
    }

    private static Member CreateMember()
    {
        return new Member(
            Guid.NewGuid(), "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
            Guid.NewGuid(), joiningReason: null, Now);
    }

    private static MemberEmployment CreateEmployment(Guid memberId)
    {
        return new MemberEmployment(Guid.NewGuid(), memberId, "BI-00123", "Immigration Officer I", Guid.NewGuid(), null);
    }

    private static UpdateMemberCommand CreateCommand()
    {
        return new UpdateMemberCommand(
            "Reyes", "Maria", "Santos", Guid.NewGuid(), new DateOnly(1985, 5, 20), "Cebu", Guid.NewGuid(),
            "Updated reason", "Senior Immigration Officer", Guid.NewGuid(), new DateOnly(2021, 1, 1));
    }

    private sealed class FakeMemberRepository : IMemberRepository
    {
        public Member? Member { get; set; }

        public MemberEmployment? Employment { get; set; }

        public bool SaveChangesCalled { get; private set; }

        public Task<bool> EmployeeNumberExistsAsync(string employeeNumber, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberProfileUpdateService.");

        public Task AddAsync(Member member, MemberEmployment employment, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberProfileUpdateService.");

        public Task<Member?> GetTrackedByIdAsync(Guid memberId, CancellationToken cancellationToken)
            => Task.FromResult(Member);

        public Task<MemberEmployment?> GetTrackedEmploymentByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
            => Task.FromResult(Employment);

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
}
