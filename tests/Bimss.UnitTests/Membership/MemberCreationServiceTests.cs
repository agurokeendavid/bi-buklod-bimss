using Bimss.Application.Auditing;
using Bimss.Application.Membership;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberCreationServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateAsync_Succeeds_AndPersistsMemberAndEmployment()
    {
        var repository = new FakeMemberRepository();
        var auditLogger = new FakeAuditLogger();
        var service = new MemberCreationService(repository, auditLogger, new FixedTimeProvider(Now));
        var actorUserId = Guid.NewGuid();
        var command = CreateCommand();

        var result = await service.CreateAsync(command, actorUserId);

        Assert.True(repository.AddAsyncCalled);
        Assert.NotNull(repository.AddedMember);
        Assert.Equal(result.MemberId, repository.AddedMember!.Id);
        Assert.Equal(command.LastName, repository.AddedMember.LastName);
        Assert.Equal(MemberStatus.PendingVerification, repository.AddedMember.Status);

        Assert.NotNull(repository.AddedEmployment);
        Assert.Equal(result.MemberId, repository.AddedEmployment!.MemberId);
        Assert.Equal(command.EmployeeNumber, repository.AddedEmployment.EmployeeNumber);

        Assert.NotNull(auditLogger.LoggedEntry);
        Assert.Equal(actorUserId, auditLogger.LoggedEntry!.ActorUserId);
        Assert.Equal("Member.Create", auditLogger.LoggedEntry.Action);
        Assert.Equal("Member", auditLogger.LoggedEntry.ObjectType);
        Assert.Equal(result.MemberId.ToString(), auditLogger.LoggedEntry.ObjectId);
        Assert.Equal(AuditResult.Success, auditLogger.LoggedEntry.Result);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenEmployeeNumberAlreadyExists()
    {
        var repository = new FakeMemberRepository { EmployeeNumberExists = true };
        var auditLogger = new FakeAuditLogger();
        var service = new MemberCreationService(repository, auditLogger, new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(CreateCommand(), Guid.NewGuid()));

        Assert.False(repository.AddAsyncCalled);
        Assert.Null(auditLogger.LoggedEntry);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenCommandIsNull()
    {
        var service = new MemberCreationService(new FakeMemberRepository(), new FakeAuditLogger(), new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateAsync(null!, Guid.NewGuid()));
    }

    private static CreateMemberCommand CreateCommand()
    {
        return new CreateMemberCommand(
            "Dela Cruz",
            "Juan",
            "Santos",
            SuffixId: null,
            new DateOnly(1990, 1, 1),
            "Manila",
            Guid.NewGuid(),
            "Referred by a colleague",
            "BI-00123",
            "Immigration Officer I",
            Guid.NewGuid(),
            new DateOnly(2020, 6, 1));
    }

    private sealed class FakeMemberRepository : IMemberRepository
    {
        public bool EmployeeNumberExists { get; set; }

        public bool AddAsyncCalled { get; private set; }

        public Member? AddedMember { get; private set; }

        public MemberEmployment? AddedEmployment { get; private set; }

        public Task<bool> EmployeeNumberExistsAsync(string employeeNumber, CancellationToken cancellationToken)
            => Task.FromResult(EmployeeNumberExists);

        public Task AddAsync(Member member, MemberEmployment employment, CancellationToken cancellationToken)
        {
            AddAsyncCalled = true;
            AddedMember = member;
            AddedEmployment = employment;
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
