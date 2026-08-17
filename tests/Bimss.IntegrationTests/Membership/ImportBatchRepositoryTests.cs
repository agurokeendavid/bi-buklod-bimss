using Bimss.Domain.Membership;
using Bimss.Infrastructure.Membership;
using Bimss.IntegrationTests.Support;

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
}
