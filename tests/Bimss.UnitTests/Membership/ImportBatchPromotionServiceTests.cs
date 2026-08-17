using Bimss.Application.Auditing;
using Bimss.Application.Membership;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class ImportBatchPromotionServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 17, 0, 0, 0, TimeSpan.Zero);
    private static readonly Guid CivilStatusId = Guid.NewGuid();
    private static readonly Guid OfficeUnitId = Guid.NewGuid();
    private static readonly Guid SuffixId = Guid.NewGuid();

    [Fact]
    public async Task PromoteRowAsync_CreatesMemberAndEmployment_FromAValidNoMatchRow()
    {
        var row = CreateEligibleRow();
        var repository = new FakeImportBatchRepository(row);
        var service = CreateService(repository);

        var result = await service.PromoteRowAsync(row.Id, Guid.NewGuid());

        Assert.NotNull(repository.PromotedMember);
        Assert.Equal(result.MemberId, repository.PromotedMember!.Id);
        Assert.Equal("Dela Cruz", repository.PromotedMember.LastName);
        Assert.Equal("Juan", repository.PromotedMember.FirstName);
        Assert.Equal(CivilStatusId, repository.PromotedMember.CivilStatusId);
        Assert.Equal(SuffixId, repository.PromotedMember.SuffixId);

        Assert.NotNull(repository.PromotedEmployment);
        Assert.Equal("BI-00123", repository.PromotedEmployment!.EmployeeNumber);
        Assert.Equal(OfficeUnitId, repository.PromotedEmployment.OfficeUnitId);

        Assert.Equal(result.MemberId, row.PromotedMemberId);
    }

    [Fact]
    public async Task PromoteRowAsync_ToleratesAnUnresolvableOptionalSuffix()
    {
        var row = CreateEligibleRow(fields => fields with { Suffix = "Not A Real Suffix" });
        var repository = new FakeImportBatchRepository(row);
        var service = CreateService(repository);

        await service.PromoteRowAsync(row.Id, Guid.NewGuid());

        Assert.Null(repository.PromotedMember!.SuffixId);
    }

    [Fact]
    public async Task PromoteRowAsync_LogsAnAuditEntry()
    {
        var row = CreateEligibleRow();
        var repository = new FakeImportBatchRepository(row);
        var auditLogger = new FakeAuditLogger();
        var service = CreateService(repository, auditLogger);
        var actorUserId = Guid.NewGuid();

        var result = await service.PromoteRowAsync(row.Id, actorUserId);

        Assert.NotNull(auditLogger.LoggedEntry);
        Assert.Equal("ImportBatch.PromoteRow", auditLogger.LoggedEntry!.Action);
        Assert.Equal(actorUserId, auditLogger.LoggedEntry.ActorUserId);
        Assert.Equal(result.MemberId.ToString(), auditLogger.LoggedEntry.ObjectId);
    }

    [Fact]
    public async Task PromoteRowAsync_Throws_WhenRowIsNotValid()
    {
        var row = CreateRow(fields => fields);
        var repository = new FakeImportBatchRepository(row);
        var service = CreateService(repository);

        await Assert.ThrowsAsync<ConflictException>(() => service.PromoteRowAsync(row.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task PromoteRowAsync_Throws_WhenRowHasNotBeenMatched()
    {
        var row = CreateRow(fields => fields);
        row.RecordValidation(isValid: true);
        var repository = new FakeImportBatchRepository(row);
        var service = CreateService(repository);

        await Assert.ThrowsAsync<ConflictException>(() => service.PromoteRowAsync(row.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task PromoteRowAsync_Throws_WhenRowIsAConfirmedDuplicate()
    {
        var row = CreateRow(fields => fields);
        row.RecordValidation(isValid: true);
        row.RecordMatch(Guid.NewGuid(), ImportRowMatchStatus.ConfirmedDuplicate);
        var repository = new FakeImportBatchRepository(row);
        var service = CreateService(repository);

        await Assert.ThrowsAsync<ConflictException>(() => service.PromoteRowAsync(row.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task PromoteRowAsync_Throws_WhenRowIsAPossibleDuplicate()
    {
        var row = CreateRow(fields => fields);
        row.RecordValidation(isValid: true);
        row.RecordMatch(Guid.NewGuid(), ImportRowMatchStatus.PossibleDuplicate);
        var repository = new FakeImportBatchRepository(row);
        var service = CreateService(repository);

        await Assert.ThrowsAsync<ConflictException>(() => service.PromoteRowAsync(row.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task PromoteRowAsync_Throws_WhenEmployeeNumberAlreadyExists()
    {
        var row = CreateEligibleRow();
        var repository = new FakeImportBatchRepository(row) { EmployeeNumberExists = true };
        var service = CreateService(repository);

        await Assert.ThrowsAsync<ConflictException>(() => service.PromoteRowAsync(row.Id, Guid.NewGuid()));
        Assert.Null(repository.PromotedMember);
    }

    [Fact]
    public async Task PromoteRowAsync_Throws_WhenCivilStatusDoesNotResolve()
    {
        var row = CreateEligibleRow(fields => fields with { CivilStatus = "Unknown" });
        var repository = new FakeImportBatchRepository(row);
        var service = CreateService(repository);

        await Assert.ThrowsAsync<DomainValidationException>(() => service.PromoteRowAsync(row.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task PromoteRowAsync_Throws_WhenDateOfBirthDoesNotParse()
    {
        var row = CreateEligibleRow(fields => fields with { DateOfBirthRaw = "not a date" });
        var repository = new FakeImportBatchRepository(row);
        var service = CreateService(repository);

        await Assert.ThrowsAsync<DomainValidationException>(() => service.PromoteRowAsync(row.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task PromoteRowAsync_Throws_WhenRowDoesNotExist()
    {
        var repository = new FakeImportBatchRepository(row: null);
        var service = CreateService(repository);

        await Assert.ThrowsAsync<NotFoundException>(() => service.PromoteRowAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    private static MemberImportStaging CreateEligibleRow(Func<MemberImportStagingFields, MemberImportStagingFields>? transform = null)
    {
        var row = CreateRow(transform);
        row.RecordValidation(isValid: true);
        row.RecordMatch(matchedMemberId: null, ImportRowMatchStatus.NoMatch);
        return row;
    }

    private static MemberImportStaging CreateRow(Func<MemberImportStagingFields, MemberImportStagingFields>? transform)
    {
        var fields = new MemberImportStagingFields
        {
            LastName = "Dela Cruz",
            FirstName = "Juan",
            PlaceOfBirth = "Manila",
            DateOfBirthRaw = "1990-01-15",
            CivilStatus = "Single",
            Suffix = "Jr.",
            EmployeeNumber = "BI-00123",
            PositionDesignation = "Immigration Officer I",
            OfficeUnit = "Port Operations Division",
            PermanentAppointmentDateRaw = "2020-06-01",
        };

        if (transform is not null)
        {
            fields = transform(fields);
        }

        return new MemberImportStaging(Guid.NewGuid(), Guid.NewGuid(), 1, fields);
    }

    private static ImportBatchPromotionService CreateService(FakeImportBatchRepository repository, FakeAuditLogger? auditLogger = null)
    {
        return new ImportBatchPromotionService(
            repository,
            new FakeMemberRepository { EmployeeNumberExists = repository.EmployeeNumberExists },
            new FakeReferenceDataQueryService(),
            auditLogger ?? new FakeAuditLogger(),
            new FixedTimeProvider(Now));
    }

    private sealed class FakeImportBatchRepository(MemberImportStaging? row) : IImportBatchRepository
    {
        public bool EmployeeNumberExists { get; set; }

        public Member? PromotedMember { get; private set; }

        public MemberEmployment? PromotedEmployment { get; private set; }

        public Task AddBatchWithRowsAsync(
            ImportBatch batch, IReadOnlyCollection<MemberImportStaging> rows, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchPromotionService.");

        public Task<ImportBatch?> GetTrackedByIdAsync(Guid importBatchId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchPromotionService.");

        public Task<IReadOnlyList<MemberImportStaging>> GetTrackedRowsByBatchIdAsync(Guid importBatchId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchPromotionService.");

        public Task AddValidationErrorsAsync(IReadOnlyCollection<ImportValidationError> errors, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchPromotionService.");

        public Task SaveChangesAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchPromotionService.");

        public Task<Guid?> FindMemberIdByEmployeeNumberAsync(string employeeNumber, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchPromotionService.");

        public Task<Guid?> FindMemberIdByNameAndDateOfBirthAsync(
            string lastName, string firstName, DateOnly dateOfBirth, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchPromotionService.");

        public Task<MemberImportStaging?> GetTrackedRowByIdAsync(Guid stagingRowId, CancellationToken cancellationToken)
            => Task.FromResult(row is not null && row.Id == stagingRowId ? row : null);

        public Task PromoteRowAsync(MemberImportStaging promotedRow, Member member, MemberEmployment employment, CancellationToken cancellationToken)
        {
            PromotedMember = member;
            PromotedEmployment = employment;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeMemberRepository : IMemberRepository
    {
        public bool EmployeeNumberExists { get; set; }

        public Task<bool> EmployeeNumberExistsAsync(string employeeNumber, CancellationToken cancellationToken)
            => Task.FromResult(EmployeeNumberExists);

        public Task AddAsync(Member member, MemberEmployment employment, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchPromotionService.");

        public Task<Member?> GetTrackedByIdAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchPromotionService.");

        public Task<MemberEmployment?> GetTrackedEmploymentByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchPromotionService.");

        public Task<bool> ExistsAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchPromotionService.");

        public Task AddDocumentAsync(MemberDocument document, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchPromotionService.");

        public Task<bool> HasAnyDocumentAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchPromotionService.");

        public Task SaveChangesAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchPromotionService.");

        public Task<MemberContact?> GetTrackedContactByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchPromotionService.");

        public Task AddContactAsync(MemberContact contact, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchPromotionService.");

        public Task<IReadOnlyList<MemberAddress>> GetTrackedAddressesByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchPromotionService.");

        public Task AddAddressAsync(MemberAddress address, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchPromotionService.");
    }

    private sealed class FakeReferenceDataQueryService : IReferenceDataQueryService
    {
        public Task<IReadOnlyList<ReferenceDataSummary>> ListCivilStatusesAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<ReferenceDataSummary>>([new ReferenceDataSummary(CivilStatusId, "SGL", "Single")]);

        public Task<IReadOnlyList<ReferenceDataSummary>> ListSuffixesAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<ReferenceDataSummary>>([new ReferenceDataSummary(SuffixId, "JR", "Jr.")]);

        public Task<IReadOnlyList<ReferenceDataSummary>> ListOfficeUnitsAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<ReferenceDataSummary>>(
                [new ReferenceDataSummary(OfficeUnitId, "POD", "Port Operations Division")]);

        public Task<IReadOnlyList<ReferenceDataSummary>> ListMemberStatusReasonsAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<ReferenceDataSummary>>([]);
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
