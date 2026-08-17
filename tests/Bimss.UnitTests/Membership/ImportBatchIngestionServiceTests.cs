using Bimss.Application.Auditing;
using Bimss.Application.Membership;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class ImportBatchIngestionServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 17, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task IngestAsync_MapsKnownColumns_AndStagesEveryRow()
    {
        var reader = new FakeWorkbookReader(
        [
            new Dictionary<string, string?>
            {
                ["Last Name"] = "Dela Cruz",
                ["First Name"] = "Juan",
                ["BI Employee Number"] = "BI-00123",
                ["Names of Children"] = "Maria (2015), Jose (2018)",
            },
            new Dictionary<string, string?>
            {
                ["Last Name"] = "Santos",
                ["First Name"] = "Ana",
            },
        ]);
        var repository = new FakeImportBatchRepository();
        var auditLogger = new FakeAuditLogger();
        var service = new ImportBatchIngestionService(reader, repository, auditLogger, new FixedTimeProvider(Now));
        var uploadedByUserId = Guid.NewGuid();
        using var content = new MemoryStream();

        var result = await service.IngestAsync("legacy-members.xlsx", content, uploadedByUserId);

        Assert.Equal(2, result.RowCount);
        Assert.NotNull(repository.AddedBatch);
        Assert.Equal(result.ImportBatchId, repository.AddedBatch!.Id);
        Assert.Equal("legacy-members.xlsx", repository.AddedBatch.FileName);
        Assert.Equal(ImportBatchStatus.Staged, repository.AddedBatch.Status);
        Assert.Equal(2, repository.AddedBatch.RowCount);

        Assert.NotNull(repository.AddedRows);
        Assert.Equal(2, repository.AddedRows!.Count);
        var firstRow = repository.AddedRows[0];
        Assert.Equal(1, firstRow.RowNumber);
        Assert.Equal("Dela Cruz", firstRow.LastName);
        Assert.Equal("Juan", firstRow.FirstName);
        Assert.Equal("BI-00123", firstRow.EmployeeNumber);
        Assert.Equal("Maria (2015), Jose (2018)", firstRow.ChildrenRaw);
        Assert.Equal(ImportRowValidationStatus.NotValidated, firstRow.ValidationStatus);

        var secondRow = repository.AddedRows[1];
        Assert.Equal(2, secondRow.RowNumber);
        Assert.Equal("Santos", secondRow.LastName);

        Assert.NotNull(auditLogger.LoggedEntry);
        Assert.Equal("ImportBatch.Ingest", auditLogger.LoggedEntry!.Action);
        Assert.Equal(result.ImportBatchId.ToString(), auditLogger.LoggedEntry.ObjectId);
    }

    [Fact]
    public async Task IngestAsync_CapturesBeneficiariesAsStructuredJson()
    {
        var reader = new FakeWorkbookReader(
        [
            new Dictionary<string, string?>
            {
                ["Beneficiary 1 — Complete Name"] = "Maria Dela Cruz",
                ["Beneficiary 1 — Relationship to Member"] = "Spouse",
                ["Additional Beneficiaries (Beneficiary 5 and above)"] = "See attached list",
            },
        ]);
        var repository = new FakeImportBatchRepository();
        var service = new ImportBatchIngestionService(reader, repository, new FakeAuditLogger(), new FixedTimeProvider(Now));
        using var content = new MemoryStream();

        await service.IngestAsync("legacy-members.xlsx", content, Guid.NewGuid());

        var row = repository.AddedRows![0];
        Assert.NotNull(row.BeneficiariesRaw);
        Assert.Contains("Maria Dela Cruz", row.BeneficiariesRaw);
        Assert.Contains("Spouse", row.BeneficiariesRaw);
        Assert.Contains("See attached list", row.BeneficiariesRaw);
    }

    [Fact]
    public async Task IngestAsync_LeavesBeneficiariesRawNull_WhenNoBeneficiaryColumnsArePresent()
    {
        var reader = new FakeWorkbookReader([new Dictionary<string, string?> { ["Last Name"] = "Dela Cruz" }]);
        var repository = new FakeImportBatchRepository();
        var service = new ImportBatchIngestionService(reader, repository, new FakeAuditLogger(), new FixedTimeProvider(Now));
        using var content = new MemoryStream();

        await service.IngestAsync("legacy-members.xlsx", content, Guid.NewGuid());

        Assert.Null(repository.AddedRows![0].BeneficiariesRaw);
    }

    [Fact]
    public async Task IngestAsync_HandlesAnEmptyWorkbook()
    {
        var reader = new FakeWorkbookReader([]);
        var repository = new FakeImportBatchRepository();
        var service = new ImportBatchIngestionService(reader, repository, new FakeAuditLogger(), new FixedTimeProvider(Now));
        using var content = new MemoryStream();

        var result = await service.IngestAsync("empty.xlsx", content, Guid.NewGuid());

        Assert.Equal(0, result.RowCount);
        Assert.Equal(ImportBatchStatus.Staged, repository.AddedBatch!.Status);
        Assert.Empty(repository.AddedRows!);
    }

    [Fact]
    public async Task IngestAsync_Throws_WhenReaderCannotParseTheFile()
    {
        var reader = new ThrowingWorkbookReader();
        var repository = new FakeImportBatchRepository();
        var service = new ImportBatchIngestionService(reader, repository, new FakeAuditLogger(), new FixedTimeProvider(Now));
        using var content = new MemoryStream();

        var exception = await Assert.ThrowsAsync<DomainValidationException>(
            () => service.IngestAsync("not-excel.xlsx", content, Guid.NewGuid()));

        Assert.Contains("File", exception.Errors.Keys);
        Assert.False(repository.AddBatchWithRowsAsyncCalled);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task IngestAsync_Throws_WhenFileNameIsMissing(string? fileName)
    {
        var service = new ImportBatchIngestionService(
            new FakeWorkbookReader([]), new FakeImportBatchRepository(), new FakeAuditLogger(), new FixedTimeProvider(Now));
        using var content = new MemoryStream();

        await Assert.ThrowsAnyAsync<ArgumentException>(() => service.IngestAsync(fileName!, content, Guid.NewGuid()));
    }

    private sealed class FakeWorkbookReader(IReadOnlyList<IReadOnlyDictionary<string, string?>> rows) : IExcelWorkbookReader
    {
        public IReadOnlyList<IReadOnlyDictionary<string, string?>> ReadRows(Stream content) => rows;
    }

    private sealed class ThrowingWorkbookReader : IExcelWorkbookReader
    {
        public IReadOnlyList<IReadOnlyDictionary<string, string?>> ReadRows(Stream content)
            => throw new InvalidOperationException("Not a valid workbook.");
    }

    private sealed class FakeImportBatchRepository : IImportBatchRepository
    {
        public bool AddBatchWithRowsAsyncCalled { get; private set; }

        public ImportBatch? AddedBatch { get; private set; }

        public IReadOnlyList<MemberImportStaging>? AddedRows { get; private set; }

        public Task AddBatchWithRowsAsync(
            ImportBatch batch, IReadOnlyCollection<MemberImportStaging> rows, CancellationToken cancellationToken)
        {
            AddBatchWithRowsAsyncCalled = true;
            AddedBatch = batch;
            AddedRows = [.. rows];
            return Task.CompletedTask;
        }

        public Task<ImportBatch?> GetTrackedByIdAsync(Guid importBatchId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchIngestionService.");

        public Task<IReadOnlyList<MemberImportStaging>> GetTrackedRowsByBatchIdAsync(Guid importBatchId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchIngestionService.");

        public Task AddValidationErrorsAsync(IReadOnlyCollection<ImportValidationError> errors, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchIngestionService.");

        public Task SaveChangesAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchIngestionService.");

        public Task<Guid?> FindMemberIdByEmployeeNumberAsync(string employeeNumber, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchIngestionService.");

        public Task<Guid?> FindMemberIdByNameAndDateOfBirthAsync(
            string lastName, string firstName, DateOnly dateOfBirth, CancellationToken cancellationToken)
            => throw new NotSupportedException("Not used by ImportBatchIngestionService.");
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
