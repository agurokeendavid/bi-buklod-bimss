using Bimss.Application.Auditing;
using Bimss.Application.Membership;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class ImportBatchMatchingServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 17, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task MatchAsync_RecordsConfirmedDuplicate_WhenEmployeeNumberMatchesAnExistingMember()
    {
        var existingMemberId = Guid.NewGuid();
        var batch = CreateValidatedBatch(out var batchId);
        var row = CreateRow(batchId, employeeNumber: "BI-00123", lastName: "Dela Cruz", firstName: "Juan", dateOfBirthRaw: "1990-01-15");
        var repository = new FakeImportBatchRepository(batch, [row])
        {
            EmployeeNumberMatches = { ["BI-00123"] = existingMemberId },
        };
        var service = new ImportBatchMatchingService(repository, new FakeAuditLogger());

        await service.MatchAsync(batchId, Guid.NewGuid());

        Assert.Equal(ImportRowMatchStatus.ConfirmedDuplicate, row.MatchStatus);
        Assert.Equal(existingMemberId, row.MatchedMemberId);
    }

    [Fact]
    public async Task MatchAsync_RecordsPossibleDuplicate_WhenNameAndDateOfBirthMatchAnExistingMember_ButNotEmployeeNumber()
    {
        var existingMemberId = Guid.NewGuid();
        var batch = CreateValidatedBatch(out var batchId);
        var row = CreateRow(batchId, employeeNumber: "BI-00999", lastName: "Dela Cruz", firstName: "Juan", dateOfBirthRaw: "1990-01-15");
        var repository = new FakeImportBatchRepository(batch, [row])
        {
            NameAndDateOfBirthMatches = { [("Dela Cruz", "Juan", new DateOnly(1990, 1, 15))] = existingMemberId },
        };
        var service = new ImportBatchMatchingService(repository, new FakeAuditLogger());

        await service.MatchAsync(batchId, Guid.NewGuid());

        Assert.Equal(ImportRowMatchStatus.PossibleDuplicate, row.MatchStatus);
        Assert.Equal(existingMemberId, row.MatchedMemberId);
    }

    [Fact]
    public async Task MatchAsync_RecordsNoMatch_WhenNothingMatches()
    {
        var batch = CreateValidatedBatch(out var batchId);
        var row = CreateRow(batchId, employeeNumber: "BI-00123", lastName: "Dela Cruz", firstName: "Juan", dateOfBirthRaw: "1990-01-15");
        var repository = new FakeImportBatchRepository(batch, [row]);
        var service = new ImportBatchMatchingService(repository, new FakeAuditLogger());

        await service.MatchAsync(batchId, Guid.NewGuid());

        Assert.Equal(ImportRowMatchStatus.NoMatch, row.MatchStatus);
        Assert.Null(row.MatchedMemberId);
    }

    [Fact]
    public async Task MatchAsync_RecordsNoMatch_WhenDateOfBirthDoesNotParse()
    {
        var batch = CreateValidatedBatch(out var batchId);
        var row = CreateRow(batchId, employeeNumber: "BI-00123", lastName: "Dela Cruz", firstName: "Juan", dateOfBirthRaw: "not a date");
        var repository = new FakeImportBatchRepository(batch, [row])
        {
            NameAndDateOfBirthMatches = { [("Dela Cruz", "Juan", new DateOnly(1990, 1, 15))] = Guid.NewGuid() },
        };
        var service = new ImportBatchMatchingService(repository, new FakeAuditLogger());

        await service.MatchAsync(batchId, Guid.NewGuid());

        Assert.Equal(ImportRowMatchStatus.NoMatch, row.MatchStatus);
    }

    [Fact]
    public async Task MatchAsync_Throws_WhenBatchIsNotValidated()
    {
        var batch = new ImportBatch(Guid.NewGuid(), "legacy-members.xlsx", Guid.NewGuid(), Now);
        var repository = new FakeImportBatchRepository(batch, []);
        var service = new ImportBatchMatchingService(repository, new FakeAuditLogger());

        await Assert.ThrowsAsync<ConflictException>(() => service.MatchAsync(batch.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task MatchAsync_Throws_WhenBatchDoesNotExist()
    {
        var repository = new FakeImportBatchRepository(batch: null, rows: []);
        var service = new ImportBatchMatchingService(repository, new FakeAuditLogger());

        await Assert.ThrowsAsync<NotFoundException>(() => service.MatchAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task MatchAsync_LogsAnAuditEntry()
    {
        var batch = CreateValidatedBatch(out var batchId);
        var repository = new FakeImportBatchRepository(batch, []);
        var auditLogger = new FakeAuditLogger();
        var service = new ImportBatchMatchingService(repository, auditLogger);
        var actorUserId = Guid.NewGuid();

        await service.MatchAsync(batchId, actorUserId);

        Assert.NotNull(auditLogger.LoggedEntry);
        Assert.Equal("ImportBatch.Match", auditLogger.LoggedEntry!.Action);
        Assert.Equal(actorUserId, auditLogger.LoggedEntry.ActorUserId);
    }

    private static ImportBatch CreateValidatedBatch(out Guid batchId)
    {
        var batch = new ImportBatch(Guid.NewGuid(), "legacy-members.xlsx", Guid.NewGuid(), Now);
        batch.MarkStaged(1, Now);
        batch.MarkValidated(Now);
        batchId = batch.Id;
        return batch;
    }

    private static MemberImportStaging CreateRow(
        Guid batchId, string employeeNumber, string lastName, string firstName, string dateOfBirthRaw)
    {
        return new MemberImportStaging(
            Guid.NewGuid(),
            batchId,
            1,
            new MemberImportStagingFields
            {
                EmployeeNumber = employeeNumber,
                LastName = lastName,
                FirstName = firstName,
                DateOfBirthRaw = dateOfBirthRaw,
            });
    }

    private sealed class FakeImportBatchRepository(ImportBatch? batch, IReadOnlyList<MemberImportStaging> rows) : IImportBatchRepository
    {
        public Dictionary<string, Guid> EmployeeNumberMatches { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<(string LastName, string FirstName, DateOnly DateOfBirth), Guid> NameAndDateOfBirthMatches { get; } = [];

        public bool SaveChangesAsyncCalled { get; private set; }

        public Task AddBatchWithRowsAsync(
            ImportBatch newBatch, IReadOnlyCollection<MemberImportStaging> newRows, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchMatchingService.");

        public Task<ImportBatch?> GetTrackedByIdAsync(Guid importBatchId, CancellationToken cancellationToken)
            => Task.FromResult(batch is not null && batch.Id == importBatchId ? batch : null);

        public Task<IReadOnlyList<MemberImportStaging>> GetTrackedRowsByBatchIdAsync(Guid importBatchId, CancellationToken cancellationToken)
            => Task.FromResult(rows);

        public Task AddValidationErrorsAsync(IReadOnlyCollection<ImportValidationError> errors, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchMatchingService.");

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesAsyncCalled = true;
            return Task.CompletedTask;
        }

        public Task<Guid?> FindMemberIdByEmployeeNumberAsync(string employeeNumber, CancellationToken cancellationToken)
            => Task.FromResult(EmployeeNumberMatches.TryGetValue(employeeNumber, out var memberId) ? memberId : (Guid?)null);

        public Task<Guid?> FindMemberIdByNameAndDateOfBirthAsync(
            string lastName, string firstName, DateOnly dateOfBirth, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                NameAndDateOfBirthMatches.TryGetValue((lastName, firstName, dateOfBirth), out var memberId) ? memberId : (Guid?)null);
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
