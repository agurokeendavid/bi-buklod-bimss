using Bimss.Domain.Membership;
using Bimss.Infrastructure.Membership;
using Bimss.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Membership;

public class ImportBatchRepositoryTests
{
    private static readonly DateTimeOffset OccurredAt = new(2026, 8, 17, 0, 0, 0, TimeSpan.Zero);

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetTrackedByIdAsync_ReturnsNull_WhenBatchDoesNotExist()
    {
        await using var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var repository = new ImportBatchRepository(dbContext);

        var batch = await repository.GetTrackedByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(batch);
    }

    [Fact]
    public async Task GetTrackedRowsByBatchIdAsync_ReturnsRows_OrderedByRowNumber()
    {
        var batchId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var repository = new ImportBatchRepository(writeContext);
            var batch = new ImportBatch(batchId, "legacy-members.xlsx", Guid.NewGuid(), OccurredAt);
            var rows = new[]
            {
                new MemberImportStaging(Guid.NewGuid(), batchId, 3, new MemberImportStagingFields { LastName = "Third" }),
                new MemberImportStaging(Guid.NewGuid(), batchId, 1, new MemberImportStagingFields { LastName = "First" }),
                new MemberImportStaging(Guid.NewGuid(), batchId, 2, new MemberImportStagingFields { LastName = "Second" }),
            };
            await repository.AddBatchWithRowsAsync(batch, rows, CancellationToken.None);
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var readRepository = new ImportBatchRepository(readContext);

        var ordered = await readRepository.GetTrackedRowsByBatchIdAsync(batchId, CancellationToken.None);

        Assert.Equal(new[] { "First", "Second", "Third" }, ordered.Select(row => row.LastName));
    }

    [Fact]
    public async Task AddValidationErrorsAsync_PersistsErrors_AfterSaveChangesAsync()
    {
        var batchId = Guid.NewGuid();
        var stagingId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var repository = new ImportBatchRepository(writeContext);
            var batch = new ImportBatch(batchId, "legacy-members.xlsx", Guid.NewGuid(), OccurredAt);
            var row = new MemberImportStaging(stagingId, batchId, 1, new MemberImportStagingFields());
            await repository.AddBatchWithRowsAsync(batch, [row], CancellationToken.None);

            await repository.AddValidationErrorsAsync(
                [
                    new ImportValidationError(
                        Guid.NewGuid(), batchId, stagingId, "LastName", ImportValidationSeverity.Error, "Last name is required.", OccurredAt),
                ],
                CancellationToken.None);
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var error = Assert.Single(readContext.ImportValidationErrors);
        Assert.Equal(stagingId, error.MemberImportStagingId);
        Assert.Equal("LastName", error.FieldName);
    }

    [Fact]
    public async Task FindMemberIdByEmployeeNumberAsync_ReturnsMemberId_ForACaseInsensitiveExactMatch()
    {
        var memberId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var member = new Member(
                memberId, "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 15), "Manila",
                Guid.NewGuid(), joiningReason: null, OccurredAt);
            writeContext.Members.Add(member);
            writeContext.MemberEmployments.Add(
                new MemberEmployment(Guid.NewGuid(), memberId, "BI-00123", "Officer I", Guid.NewGuid(), null));
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var repository = new ImportBatchRepository(readContext);

        var found = await repository.FindMemberIdByEmployeeNumberAsync("bi-00123", CancellationToken.None);

        Assert.Equal(memberId, found);
    }

    [Fact]
    public async Task FindMemberIdByEmployeeNumberAsync_ReturnsNull_WhenNoMemberHasThatNumber()
    {
        await using var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var repository = new ImportBatchRepository(dbContext);

        var found = await repository.FindMemberIdByEmployeeNumberAsync("BI-99999", CancellationToken.None);

        Assert.Null(found);
    }

    [Fact]
    public async Task FindMemberIdByNameAndDateOfBirthAsync_ReturnsMemberId_ForACaseInsensitiveExactMatch()
    {
        var memberId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var member = new Member(
                memberId, "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 15), "Manila",
                Guid.NewGuid(), joiningReason: null, OccurredAt);
            writeContext.Members.Add(member);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var repository = new ImportBatchRepository(readContext);

        var found = await repository.FindMemberIdByNameAndDateOfBirthAsync(
            "dela cruz", "juan", new DateOnly(1990, 1, 15), CancellationToken.None);

        Assert.Equal(memberId, found);
    }

    [Fact]
    public async Task FindMemberIdByNameAndDateOfBirthAsync_ReturnsNull_WhenDateOfBirthDiffers()
    {
        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.Members.Add(new Member(
                Guid.NewGuid(), "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 15), "Manila",
                Guid.NewGuid(), joiningReason: null, OccurredAt));
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var repository = new ImportBatchRepository(readContext);

        var found = await repository.FindMemberIdByNameAndDateOfBirthAsync(
            "Dela Cruz", "Juan", new DateOnly(1991, 2, 20), CancellationToken.None);

        Assert.Null(found);
    }

    [Fact]
    public async Task GetTrackedRowByIdAsync_ReturnsNull_WhenRowDoesNotExist()
    {
        await using var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var repository = new ImportBatchRepository(dbContext);

        var row = await repository.GetTrackedRowByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(row);
    }

    [Fact]
    public async Task PromoteRowAsync_PersistsMemberEmploymentAndTheUpdatedRow_Atomically()
    {
        var batchId = Guid.NewGuid();
        var stagingId = Guid.NewGuid();
        var civilStatusId = Guid.NewGuid();
        var officeUnitId = Guid.NewGuid();
        Guid memberId;

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var repository = new ImportBatchRepository(writeContext);
            var batch = new ImportBatch(batchId, "legacy-members.xlsx", Guid.NewGuid(), OccurredAt);
            var row = new MemberImportStaging(stagingId, batchId, 1, new MemberImportStagingFields { LastName = "Dela Cruz" });
            await repository.AddBatchWithRowsAsync(batch, [row], CancellationToken.None);

            var trackedRow = await repository.GetTrackedRowByIdAsync(stagingId, CancellationToken.None);
            Assert.NotNull(trackedRow);
            trackedRow!.RecordValidation(isValid: true);
            trackedRow.RecordMatch(matchedMemberId: null, ImportRowMatchStatus.NoMatch);

            var member = new Member(
                Guid.NewGuid(), "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 15), "Manila",
                civilStatusId, joiningReason: null, OccurredAt);
            memberId = member.Id;
            var employment = new MemberEmployment(Guid.NewGuid(), memberId, "BI-00123", "Officer I", officeUnitId, null);
            trackedRow.MarkPromoted(memberId);

            await repository.PromoteRowAsync(trackedRow, member, employment, CancellationToken.None);
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        Assert.True(await readContext.Members.AnyAsync(m => m.Id == memberId));
        Assert.True(await readContext.MemberEmployments.AnyAsync(e => e.MemberId == memberId));
        var persistedRow = await readContext.MemberImportStagingRows.SingleAsync(r => r.Id == stagingId);
        Assert.Equal(memberId, persistedRow.PromotedMemberId);
    }
}
