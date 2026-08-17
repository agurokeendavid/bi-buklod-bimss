using Bimss.Application.Auditing;
using Bimss.Application.Membership;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class ImportBatchValidationServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 17, 0, 0, 0, TimeSpan.Zero);
    private static readonly Guid CivilStatusId = Guid.NewGuid();
    private static readonly Guid OfficeUnitId = Guid.NewGuid();
    private static readonly Guid SuffixId = Guid.NewGuid();

    [Fact]
    public async Task ValidateAsync_MarksRowValid_WhenAllRequiredFieldsAreWellFormed()
    {
        var batch = CreateStagedBatch(out var batchId);
        var row = CreateRow(batchId, new MemberImportStagingFields
        {
            LastName = "Dela Cruz",
            FirstName = "Juan",
            PlaceOfBirth = "Manila",
            DateOfBirthRaw = "1990-01-15",
            CivilStatus = "Single",
            EmployeeNumber = "BI-00123",
            PositionDesignation = "Immigration Officer I",
            OfficeUnit = "Port Operations Division",
        });
        var repository = new FakeImportBatchRepository(batch, [row]);
        var service = CreateService(repository);

        await service.ValidateAsync(batchId, Guid.NewGuid());

        Assert.Equal(ImportRowValidationStatus.Valid, row.ValidationStatus);
        Assert.Empty(repository.AddedErrors);
        Assert.Equal(ImportBatchStatus.Validated, batch.Status);
    }

    [Fact]
    public async Task ValidateAsync_MarksRowInvalid_AndRecordsErrors_WhenRequiredFieldsAreMissing()
    {
        var batch = CreateStagedBatch(out var batchId);
        var row = CreateRow(batchId, new MemberImportStagingFields());
        var repository = new FakeImportBatchRepository(batch, [row]);
        var service = CreateService(repository);

        await service.ValidateAsync(batchId, Guid.NewGuid());

        Assert.Equal(ImportRowValidationStatus.Invalid, row.ValidationStatus);
        Assert.NotEmpty(repository.AddedErrors);
        Assert.Contains(repository.AddedErrors, error => error.FieldName == nameof(MemberImportStaging.LastName));
        Assert.Contains(repository.AddedErrors, error => error.FieldName == nameof(MemberImportStaging.EmployeeNumber));
        Assert.All(repository.AddedErrors, error => Assert.Equal(row.Id, error.MemberImportStagingId));
    }

    [Fact]
    public async Task ValidateAsync_RecordsError_WhenDateOfBirthIsNotAValidDate()
    {
        var batch = CreateStagedBatch(out var batchId);
        var row = CreateRow(batchId, ValidFields() with { DateOfBirthRaw = "not a date" });
        var repository = new FakeImportBatchRepository(batch, [row]);
        var service = CreateService(repository);

        await service.ValidateAsync(batchId, Guid.NewGuid());

        Assert.Equal(ImportRowValidationStatus.Invalid, row.ValidationStatus);
        Assert.Contains(
            repository.AddedErrors,
            error => error.FieldName == nameof(MemberImportStaging.DateOfBirthRaw) && error.Severity == ImportValidationSeverity.Error);
    }

    [Fact]
    public async Task ValidateAsync_RecordsError_WhenCivilStatusDoesNotMatchKnownReferenceData()
    {
        var batch = CreateStagedBatch(out var batchId);
        var row = CreateRow(batchId, ValidFields() with { CivilStatus = "Unknown Status" });
        var repository = new FakeImportBatchRepository(batch, [row]);
        var service = CreateService(repository);

        await service.ValidateAsync(batchId, Guid.NewGuid());

        Assert.Equal(ImportRowValidationStatus.Invalid, row.ValidationStatus);
        Assert.Contains(
            repository.AddedErrors,
            error => error.FieldName == nameof(MemberImportStaging.CivilStatus) && error.Severity == ImportValidationSeverity.Error);
    }

    [Fact]
    public async Task ValidateAsync_RecordsWarningOnly_WhenSuffixDoesNotMatchKnownReferenceData()
    {
        var batch = CreateStagedBatch(out var batchId);
        var row = CreateRow(batchId, ValidFields() with { Suffix = "Not A Real Suffix" });
        var repository = new FakeImportBatchRepository(batch, [row]);
        var service = CreateService(repository);

        await service.ValidateAsync(batchId, Guid.NewGuid());

        Assert.Equal(ImportRowValidationStatus.Valid, row.ValidationStatus);
        Assert.Contains(
            repository.AddedErrors,
            error => error.FieldName == nameof(MemberImportStaging.Suffix) && error.Severity == ImportValidationSeverity.Warning);
    }

    [Fact]
    public async Task ValidateAsync_DoesNotRequireSuffixOrPermanentAppointmentDate()
    {
        var batch = CreateStagedBatch(out var batchId);
        var row = CreateRow(batchId, ValidFields() with { Suffix = null, PermanentAppointmentDateRaw = null });
        var repository = new FakeImportBatchRepository(batch, [row]);
        var service = CreateService(repository);

        await service.ValidateAsync(batchId, Guid.NewGuid());

        Assert.Equal(ImportRowValidationStatus.Valid, row.ValidationStatus);
    }

    [Fact]
    public async Task ValidateAsync_MarksTheBatchValidated()
    {
        var batch = CreateStagedBatch(out var batchId);
        var repository = new FakeImportBatchRepository(batch, []);
        var service = CreateService(repository);

        await service.ValidateAsync(batchId, Guid.NewGuid());

        Assert.Equal(ImportBatchStatus.Validated, batch.Status);
        Assert.True(repository.SaveChangesAsyncCalled);
    }

    [Fact]
    public async Task ValidateAsync_LogsAnAuditEntry()
    {
        var batch = CreateStagedBatch(out var batchId);
        var repository = new FakeImportBatchRepository(batch, []);
        var auditLogger = new FakeAuditLogger();
        var service = CreateService(repository, auditLogger);
        var actorUserId = Guid.NewGuid();

        await service.ValidateAsync(batchId, actorUserId);

        Assert.NotNull(auditLogger.LoggedEntry);
        Assert.Equal("ImportBatch.Validate", auditLogger.LoggedEntry!.Action);
        Assert.Equal(actorUserId, auditLogger.LoggedEntry.ActorUserId);
        Assert.Equal(batchId.ToString(), auditLogger.LoggedEntry.ObjectId);
    }

    [Fact]
    public async Task ValidateAsync_Throws_WhenBatchDoesNotExist()
    {
        var repository = new FakeImportBatchRepository(batch: null, rows: []);
        var service = CreateService(repository);

        await Assert.ThrowsAsync<NotFoundException>(() => service.ValidateAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    private static MemberImportStagingFields ValidFields()
    {
        return new MemberImportStagingFields
        {
            LastName = "Dela Cruz",
            FirstName = "Juan",
            PlaceOfBirth = "Manila",
            DateOfBirthRaw = "1990-01-15",
            CivilStatus = "Single",
            EmployeeNumber = "BI-00123",
            PositionDesignation = "Immigration Officer I",
            OfficeUnit = "Port Operations Division",
            Suffix = "Jr.",
            PermanentAppointmentDateRaw = "2020-06-01",
        };
    }

    private static ImportBatchValidationService CreateService(FakeImportBatchRepository repository, FakeAuditLogger? auditLogger = null)
    {
        return new ImportBatchValidationService(
            repository, new FakeReferenceDataQueryService(), auditLogger ?? new FakeAuditLogger(), new FixedTimeProvider(Now));
    }

    private static ImportBatch CreateStagedBatch(out Guid batchId)
    {
        var batch = new ImportBatch(Guid.NewGuid(), "legacy-members.xlsx", Guid.NewGuid(), Now);
        batch.MarkStaged(1, Now);
        batchId = batch.Id;
        return batch;
    }

    private static MemberImportStaging CreateRow(Guid batchId, MemberImportStagingFields fields)
    {
        return new MemberImportStaging(Guid.NewGuid(), batchId, 1, fields);
    }

    private sealed class FakeImportBatchRepository(ImportBatch? batch, IReadOnlyList<MemberImportStaging> rows) : IImportBatchRepository
    {
        public List<ImportValidationError> AddedErrors { get; } = [];

        public bool SaveChangesAsyncCalled { get; private set; }

        public Task AddBatchWithRowsAsync(
            ImportBatch newBatch, IReadOnlyCollection<MemberImportStaging> newRows, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchValidationService.");

        public Task<ImportBatch?> GetTrackedByIdAsync(Guid importBatchId, CancellationToken cancellationToken)
            => Task.FromResult(batch is not null && batch.Id == importBatchId ? batch : null);

        public Task<IReadOnlyList<MemberImportStaging>> GetTrackedRowsByBatchIdAsync(Guid importBatchId, CancellationToken cancellationToken)
            => Task.FromResult(rows);

        public Task AddValidationErrorsAsync(IReadOnlyCollection<ImportValidationError> errors, CancellationToken cancellationToken)
        {
            AddedErrors.AddRange(errors);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesAsyncCalled = true;
            return Task.CompletedTask;
        }
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
