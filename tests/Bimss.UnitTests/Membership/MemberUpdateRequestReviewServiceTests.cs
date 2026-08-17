using Bimss.Application.Auditing;
using Bimss.Application.Membership;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberUpdateRequestReviewServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 17, 0, 0, 0, TimeSpan.Zero);
    private static readonly Guid MemberId = Guid.NewGuid();
    private static readonly Guid CivilStatusId = Guid.NewGuid();
    private static readonly Guid OfficeUnitId = Guid.NewGuid();

    [Fact]
    public async Task ApproveAsync_AppliesTheChange_AndMarksTheRequestApproved()
    {
        var member = CreateMember();
        var employment = CreateEmployment();
        var memberRepository = new FakeMemberRepository(member, employment);
        var request = CreateRequest([new MemberUpdateRequestChangeInput(nameof(Member.FirstName), "Juan", "Juanito")]);
        var updateRequestRepository = new FakeMemberUpdateRequestRepository(request);
        var auditLogger = new FakeAuditLogger();
        var service = CreateService(updateRequestRepository, memberRepository, auditLogger);
        var actorUserId = Guid.NewGuid();

        await service.ApproveAsync(request.Id, actorUserId, "Confirmed with member");

        Assert.Equal(MemberUpdateRequestStatus.Approved, request.Status);
        Assert.Equal(actorUserId, request.ReviewedByUserId);
        Assert.Equal("Confirmed with member", request.ReviewRemarks);
        Assert.Equal("Juanito", member.FirstName);
        Assert.True(updateRequestRepository.SaveChangesAsyncCalled);

        Assert.Equal(2, auditLogger.LoggedEntries.Count);
        Assert.Contains(auditLogger.LoggedEntries, e => e.Action == "Member.UpdateProfile");
        Assert.Contains(auditLogger.LoggedEntries, e => e.Action == "MemberUpdateRequest.Approve");
    }

    [Fact]
    public async Task ApproveAsync_AppliesMultipleChanges()
    {
        var member = CreateMember();
        var employment = CreateEmployment();
        var memberRepository = new FakeMemberRepository(member, employment);
        var newOfficeUnitId = Guid.NewGuid();
        var request = CreateRequest(
        [
            new MemberUpdateRequestChangeInput(nameof(Member.PlaceOfBirth), "Manila", "Cebu"),
            new MemberUpdateRequestChangeInput(nameof(MemberEmployment.OfficeUnitId), OfficeUnitId.ToString(), newOfficeUnitId.ToString()),
        ]);
        var updateRequestRepository = new FakeMemberUpdateRequestRepository(request);
        var service = CreateService(updateRequestRepository, memberRepository, new FakeAuditLogger());

        await service.ApproveAsync(request.Id, Guid.NewGuid(), remarks: null);

        Assert.Equal("Cebu", member.PlaceOfBirth);
        Assert.Equal(newOfficeUnitId, employment.OfficeUnitId);
    }

    [Fact]
    public async Task ApproveAsync_Throws_WhenRequestIsNotPending()
    {
        var member = CreateMember();
        var employment = CreateEmployment();
        var memberRepository = new FakeMemberRepository(member, employment);
        var request = CreateRequest([new MemberUpdateRequestChangeInput(nameof(Member.FirstName), "Juan", "Juanito")]);
        request.Approve(Guid.NewGuid(), Now);
        var updateRequestRepository = new FakeMemberUpdateRequestRepository(request);
        var service = CreateService(updateRequestRepository, memberRepository, new FakeAuditLogger());

        await Assert.ThrowsAsync<ConflictException>(() => service.ApproveAsync(request.Id, Guid.NewGuid(), remarks: null));
    }

    [Fact]
    public async Task ApproveAsync_Throws_WhenRequestDoesNotExist()
    {
        var updateRequestRepository = new FakeMemberUpdateRequestRepository(request: null);
        var service = CreateService(updateRequestRepository, new FakeMemberRepository(null, null), new FakeAuditLogger());

        await Assert.ThrowsAsync<NotFoundException>(() => service.ApproveAsync(Guid.NewGuid(), Guid.NewGuid(), remarks: null));
    }

    [Fact]
    public async Task RejectAsync_MarksTheRequestRejected_AndDoesNotChangeTheMember()
    {
        var member = CreateMember();
        var employment = CreateEmployment();
        var memberRepository = new FakeMemberRepository(member, employment);
        var request = CreateRequest([new MemberUpdateRequestChangeInput(nameof(Member.FirstName), "Juan", "Juanito")]);
        var updateRequestRepository = new FakeMemberUpdateRequestRepository(request);
        var auditLogger = new FakeAuditLogger();
        var service = CreateService(updateRequestRepository, memberRepository, auditLogger);
        var actorUserId = Guid.NewGuid();

        await service.RejectAsync(request.Id, actorUserId, "Name does not match submitted ID");

        Assert.Equal(MemberUpdateRequestStatus.Rejected, request.Status);
        Assert.Equal("Juan", member.FirstName);
        Assert.Single(auditLogger.LoggedEntries);
        Assert.Equal("MemberUpdateRequest.Reject", auditLogger.LoggedEntries[0].Action);
    }

    [Fact]
    public async Task RejectAsync_Throws_WhenRequestIsNotPending()
    {
        var request = CreateRequest([new MemberUpdateRequestChangeInput(nameof(Member.FirstName), "Juan", "Juanito")]);
        request.Reject(Guid.NewGuid(), Now, "First rejection");
        var updateRequestRepository = new FakeMemberUpdateRequestRepository(request);
        var service = CreateService(updateRequestRepository, new FakeMemberRepository(null, null), new FakeAuditLogger());

        await Assert.ThrowsAsync<ConflictException>(() => service.RejectAsync(request.Id, Guid.NewGuid(), "Second rejection"));
    }

    private static MemberUpdateRequestReviewService CreateService(
        FakeMemberUpdateRequestRepository updateRequestRepository, FakeMemberRepository memberRepository, FakeAuditLogger auditLogger)
    {
        var profileUpdateService = new MemberProfileUpdateService(memberRepository, auditLogger);
        return new MemberUpdateRequestReviewService(
            updateRequestRepository, memberRepository, profileUpdateService, auditLogger, new FixedTimeProvider(Now));
    }

    private static MemberUpdateRequest CreateRequest(IReadOnlyCollection<MemberUpdateRequestChangeInput> changes)
    {
        return new MemberUpdateRequest(Guid.NewGuid(), MemberId, Guid.NewGuid(), Now, changes);
    }

    private static Member CreateMember()
    {
        return new Member(
            MemberId, "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
            CivilStatusId, joiningReason: null, Now);
    }

    private static MemberEmployment CreateEmployment()
    {
        return new MemberEmployment(Guid.NewGuid(), MemberId, "BI-00123", "Immigration Officer I", OfficeUnitId, null);
    }

    private sealed class FakeMemberUpdateRequestRepository(MemberUpdateRequest? request) : IMemberUpdateRequestRepository
    {
        public bool SaveChangesAsyncCalled { get; private set; }

        public Task AddAsync(MemberUpdateRequest newRequest, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestReviewService.");

        public Task<MemberUpdateRequest?> GetTrackedByIdAsync(Guid requestId, CancellationToken cancellationToken)
            => Task.FromResult(request is not null && request.Id == requestId ? request : null);

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesAsyncCalled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeMemberRepository(Member? member, MemberEmployment? employment) : IMemberRepository
    {
        public Task<bool> EmployeeNumberExistsAsync(string employeeNumber, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestReviewService.");

        public Task AddAsync(Member newMember, MemberEmployment newEmployment, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestReviewService.");

        public Task<Member?> GetTrackedByIdAsync(Guid memberId, CancellationToken cancellationToken)
            => Task.FromResult(member is not null && member.Id == memberId ? member : null);

        public Task<MemberEmployment?> GetTrackedEmploymentByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
            => Task.FromResult(employment is not null && employment.MemberId == memberId ? employment : null);

        public Task<bool> ExistsAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestReviewService.");

        public Task AddDocumentAsync(MemberDocument document, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestReviewService.");

        public Task<bool> HasAnyDocumentAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestReviewService.");

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<MemberContact?> GetTrackedContactByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestReviewService.");

        public Task AddContactAsync(MemberContact contact, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestReviewService.");

        public Task<IReadOnlyList<MemberAddress>> GetTrackedAddressesByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestReviewService.");

        public Task AddAddressAsync(MemberAddress address, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestReviewService.");
    }

    private sealed class FakeAuditLogger : IAuditLogger
    {
        public List<AuditEntry> LoggedEntries { get; } = [];

        public Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default)
        {
            LoggedEntries.Add(entry);
            return Task.CompletedTask;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
