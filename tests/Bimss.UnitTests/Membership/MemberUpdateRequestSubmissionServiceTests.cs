using Bimss.Application.Auditing;
using Bimss.Application.Membership;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberUpdateRequestSubmissionServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 17, 0, 0, 0, TimeSpan.Zero);
    private static readonly Guid MemberId = Guid.NewGuid();
    private static readonly Guid CivilStatusId = Guid.NewGuid();
    private static readonly Guid OfficeUnitId = Guid.NewGuid();

    [Fact]
    public async Task SubmitAsync_RecordsOnlyTheFieldsThatChanged()
    {
        var member = CreateMember();
        var employment = CreateEmployment();
        var repository = new FakeMemberRepository(member, employment);
        var updateRequestRepository = new FakeMemberUpdateRequestRepository();
        var service = CreateService(repository, updateRequestRepository);
        var submittedByUserId = Guid.NewGuid();

        var command = new UpdateMemberCommand(
            member.LastName,
            "Juanito",
            member.MiddleName,
            member.SuffixId,
            member.DateOfBirth,
            member.PlaceOfBirth,
            member.CivilStatusId,
            member.JoiningReason,
            employment.PositionDesignation,
            employment.OfficeUnitId,
            employment.PermanentAppointmentDate);

        var requestId = await service.SubmitAsync(MemberId, submittedByUserId, command);

        Assert.NotNull(updateRequestRepository.AddedRequest);
        Assert.Equal(requestId, updateRequestRepository.AddedRequest!.Id);
        Assert.Equal(MemberId, updateRequestRepository.AddedRequest.MemberId);
        Assert.Equal(submittedByUserId, updateRequestRepository.AddedRequest.SubmittedByUserId);

        var change = Assert.Single(updateRequestRepository.AddedRequest.Changes);
        Assert.Equal(nameof(Member.FirstName), change.FieldName);
        Assert.Equal("Juan", change.OldValue);
        Assert.Equal("Juanito", change.NewValue);
    }

    [Fact]
    public async Task SubmitAsync_RecordsMultipleChangedFields()
    {
        var member = CreateMember();
        var employment = CreateEmployment();
        var repository = new FakeMemberRepository(member, employment);
        var updateRequestRepository = new FakeMemberUpdateRequestRepository();
        var service = CreateService(repository, updateRequestRepository);
        var newOfficeUnitId = Guid.NewGuid();

        var command = new UpdateMemberCommand(
            member.LastName,
            "Juanito",
            member.MiddleName,
            member.SuffixId,
            member.DateOfBirth,
            "Cebu",
            member.CivilStatusId,
            member.JoiningReason,
            employment.PositionDesignation,
            newOfficeUnitId,
            employment.PermanentAppointmentDate);

        await service.SubmitAsync(MemberId, Guid.NewGuid(), command);

        Assert.Equal(3, updateRequestRepository.AddedRequest!.Changes.Count);
        Assert.Contains(updateRequestRepository.AddedRequest.Changes, c => c.FieldName == nameof(Member.FirstName));
        Assert.Contains(updateRequestRepository.AddedRequest.Changes, c => c.FieldName == nameof(Member.PlaceOfBirth));
        Assert.Contains(updateRequestRepository.AddedRequest.Changes, c => c.FieldName == nameof(MemberEmployment.OfficeUnitId));
    }

    [Fact]
    public async Task SubmitAsync_LogsAnAuditEntry()
    {
        var member = CreateMember();
        var employment = CreateEmployment();
        var repository = new FakeMemberRepository(member, employment);
        var auditLogger = new FakeAuditLogger();
        var service = CreateService(repository, new FakeMemberUpdateRequestRepository(), auditLogger);
        var submittedByUserId = Guid.NewGuid();

        var requestId = await service.SubmitAsync(MemberId, submittedByUserId, CommandWithChangedFirstName(member, employment));

        Assert.NotNull(auditLogger.LoggedEntry);
        Assert.Equal("MemberUpdateRequest.Submit", auditLogger.LoggedEntry!.Action);
        Assert.Equal(submittedByUserId, auditLogger.LoggedEntry.ActorUserId);
        Assert.Equal(requestId.ToString(), auditLogger.LoggedEntry.ObjectId);
    }

    [Fact]
    public async Task SubmitAsync_Throws_WhenNoFieldsChanged()
    {
        var member = CreateMember();
        var employment = CreateEmployment();
        var repository = new FakeMemberRepository(member, employment);
        var updateRequestRepository = new FakeMemberUpdateRequestRepository();
        var service = CreateService(repository, updateRequestRepository);

        var command = new UpdateMemberCommand(
            member.LastName,
            member.FirstName,
            member.MiddleName,
            member.SuffixId,
            member.DateOfBirth,
            member.PlaceOfBirth,
            member.CivilStatusId,
            member.JoiningReason,
            employment.PositionDesignation,
            employment.OfficeUnitId,
            employment.PermanentAppointmentDate);

        await Assert.ThrowsAsync<DomainValidationException>(() => service.SubmitAsync(MemberId, Guid.NewGuid(), command));
        Assert.Null(updateRequestRepository.AddedRequest);
    }

    [Fact]
    public async Task SubmitAsync_Throws_WhenMemberDoesNotExist()
    {
        var repository = new FakeMemberRepository(member: null, employment: null);
        var service = CreateService(repository, new FakeMemberUpdateRequestRepository());

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.SubmitAsync(Guid.NewGuid(), Guid.NewGuid(), CommandWithChangedFirstName(CreateMember(), CreateEmployment())));
    }

    [Fact]
    public async Task SubmitAsync_Throws_WhenCommandIsNull()
    {
        var service = CreateService(new FakeMemberRepository(CreateMember(), CreateEmployment()), new FakeMemberUpdateRequestRepository());

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.SubmitAsync(MemberId, Guid.NewGuid(), null!));
    }

    private static UpdateMemberCommand CommandWithChangedFirstName(Member member, MemberEmployment employment)
    {
        return new UpdateMemberCommand(
            member.LastName,
            "Juanito",
            member.MiddleName,
            member.SuffixId,
            member.DateOfBirth,
            member.PlaceOfBirth,
            member.CivilStatusId,
            member.JoiningReason,
            employment.PositionDesignation,
            employment.OfficeUnitId,
            employment.PermanentAppointmentDate);
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

    private static MemberUpdateRequestSubmissionService CreateService(
        FakeMemberRepository repository, FakeMemberUpdateRequestRepository updateRequestRepository, FakeAuditLogger? auditLogger = null)
    {
        return new MemberUpdateRequestSubmissionService(
            repository, updateRequestRepository, auditLogger ?? new FakeAuditLogger(), new FixedTimeProvider(Now));
    }

    private sealed class FakeMemberRepository(Member? member, MemberEmployment? employment) : IMemberRepository
    {
        public Task<bool> EmployeeNumberExistsAsync(string employeeNumber, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestSubmissionService.");

        public Task AddAsync(Member newMember, MemberEmployment newEmployment, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestSubmissionService.");

        public Task<Member?> GetTrackedByIdAsync(Guid memberId, CancellationToken cancellationToken)
            => Task.FromResult(member is not null && member.Id == memberId ? member : null);

        public Task<MemberEmployment?> GetTrackedEmploymentByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
            => Task.FromResult(employment is not null && employment.MemberId == memberId ? employment : null);

        public Task<bool> ExistsAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestSubmissionService.");

        public Task AddDocumentAsync(MemberDocument document, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestSubmissionService.");

        public Task<bool> HasAnyDocumentAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestSubmissionService.");

        public Task SaveChangesAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestSubmissionService.");

        public Task<MemberContact?> GetTrackedContactByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestSubmissionService.");

        public Task AddContactAsync(MemberContact contact, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestSubmissionService.");

        public Task<IReadOnlyList<MemberAddress>> GetTrackedAddressesByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestSubmissionService.");

        public Task AddAddressAsync(MemberAddress address, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestSubmissionService.");
    }

    private sealed class FakeMemberUpdateRequestRepository : IMemberUpdateRequestRepository
    {
        public MemberUpdateRequest? AddedRequest { get; private set; }

        public Task AddAsync(MemberUpdateRequest request, CancellationToken cancellationToken)
        {
            AddedRequest = request;
            return Task.CompletedTask;
        }

        public Task<MemberUpdateRequest?> GetTrackedByIdAsync(Guid requestId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestSubmissionService.");

        public Task SaveChangesAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by MemberUpdateRequestSubmissionService.");
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
