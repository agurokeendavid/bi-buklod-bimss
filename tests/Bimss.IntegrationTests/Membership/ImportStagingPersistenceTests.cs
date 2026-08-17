using Bimss.Domain.Membership;
using Bimss.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Membership;

public class ImportStagingPersistenceTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();
    private static readonly DateTimeOffset OccurredAt = new(2026, 8, 17, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ImportBatch_RoundTrips_ThroughPersistence()
    {
        var id = Guid.NewGuid();
        var uploadedByUserId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var batch = new ImportBatch(id, "legacy-members.xlsx", uploadedByUserId, OccurredAt);
            writeContext.ImportBatches.Add(batch);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.ImportBatches.SingleAsync();

        Assert.Equal(id, persisted.Id);
        Assert.Equal("legacy-members.xlsx", persisted.FileName);
        Assert.Equal(uploadedByUserId, persisted.UploadedByUserId);
        Assert.Equal(ImportBatchStatus.Created, persisted.Status);
    }

    [Fact]
    public async Task ImportBatch_StatusTransitions_PersistAcrossReloads()
    {
        var id = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var batch = new ImportBatch(id, "legacy-members.xlsx", Guid.NewGuid(), OccurredAt);
            writeContext.ImportBatches.Add(batch);
            await writeContext.SaveChangesAsync();
        }

        await using (var stageContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var batch = await stageContext.ImportBatches.SingleAsync();
            batch.MarkStaged(5, OccurredAt);
            await stageContext.SaveChangesAsync();
        }

        await using (var validateContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var batch = await validateContext.ImportBatches.SingleAsync();
            batch.MarkValidated(OccurredAt);
            await validateContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.ImportBatches.SingleAsync();

        Assert.Equal(ImportBatchStatus.Validated, persisted.Status);
        Assert.Equal(5, persisted.RowCount);
        Assert.NotNull(persisted.StagedAtUtc);
        Assert.NotNull(persisted.ValidatedAtUtc);
    }

    [Fact]
    public async Task MemberImportStaging_RoundTrips_ThroughPersistence()
    {
        var batchId = Guid.NewGuid();
        var rowId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var batch = new ImportBatch(batchId, "legacy-members.xlsx", Guid.NewGuid(), OccurredAt);
            writeContext.ImportBatches.Add(batch);

            var row = new MemberImportStaging(
                rowId,
                batchId,
                1,
                new MemberImportStagingFields { LastName = "Dela Cruz", FirstName = "Juan", EmployeeNumber = "BI-00123" });
            writeContext.MemberImportStagingRows.Add(row);

            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.MemberImportStagingRows.SingleAsync();

        Assert.Equal(rowId, persisted.Id);
        Assert.Equal(batchId, persisted.ImportBatchId);
        Assert.Equal(1, persisted.RowNumber);
        Assert.Equal("Dela Cruz", persisted.LastName);
        Assert.Equal("Juan", persisted.FirstName);
        Assert.Equal("BI-00123", persisted.EmployeeNumber);
        Assert.Equal(ImportRowValidationStatus.NotValidated, persisted.ValidationStatus);
    }

    [Fact]
    public async Task MemberImportStaging_ValidationAndPromotion_PersistAcrossReloads()
    {
        var batchId = Guid.NewGuid();
        var rowId = Guid.NewGuid();
        var promotedMemberId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.ImportBatches.Add(new ImportBatch(batchId, "legacy-members.xlsx", Guid.NewGuid(), OccurredAt));
            writeContext.MemberImportStagingRows.Add(
                new MemberImportStaging(rowId, batchId, 1, new MemberImportStagingFields { LastName = "Dela Cruz" }));
            await writeContext.SaveChangesAsync();
        }

        await using (var validateContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var row = await validateContext.MemberImportStagingRows.SingleAsync();
            row.RecordValidation(isValid: true);
            row.RecordMatch(matchedMemberId: null, ImportRowMatchStatus.NoMatch);
            await validateContext.SaveChangesAsync();
        }

        await using (var promoteContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var row = await promoteContext.MemberImportStagingRows.SingleAsync();
            row.MarkPromoted(promotedMemberId);
            await promoteContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.MemberImportStagingRows.SingleAsync();

        Assert.Equal(ImportRowValidationStatus.Valid, persisted.ValidationStatus);
        Assert.Equal(ImportRowMatchStatus.NoMatch, persisted.MatchStatus);
        Assert.Equal(promotedMemberId, persisted.PromotedMemberId);
    }

    [Fact]
    public async Task ImportValidationError_RoundTrips_ThroughPersistence()
    {
        var batchId = Guid.NewGuid();
        var rowId = Guid.NewGuid();
        var errorId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.ImportBatches.Add(new ImportBatch(batchId, "legacy-members.xlsx", Guid.NewGuid(), OccurredAt));
            writeContext.MemberImportStagingRows.Add(
                new MemberImportStaging(rowId, batchId, 1, new MemberImportStagingFields()));
            writeContext.ImportValidationErrors.Add(new ImportValidationError(
                errorId, batchId, rowId, "EmployeeNumber", ImportValidationSeverity.Error, "Employee number is required.", OccurredAt));
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.ImportValidationErrors.SingleAsync();

        Assert.Equal(errorId, persisted.Id);
        Assert.Equal(batchId, persisted.ImportBatchId);
        Assert.Equal(rowId, persisted.MemberImportStagingId);
        Assert.Equal("EmployeeNumber", persisted.FieldName);
        Assert.Equal(ImportValidationSeverity.Error, persisted.Severity);
        Assert.Equal("Employee number is required.", persisted.Message);
    }
}
