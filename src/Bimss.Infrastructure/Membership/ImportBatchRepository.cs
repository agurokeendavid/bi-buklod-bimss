using Bimss.Application.Membership;
using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.Infrastructure.Membership;

public sealed class ImportBatchRepository(BimssDbContext dbContext) : IImportBatchRepository
{
    public async Task AddBatchWithRowsAsync(
        ImportBatch batch, IReadOnlyCollection<MemberImportStaging> rows, CancellationToken cancellationToken)
    {
        dbContext.ImportBatches.Add(batch);
        dbContext.MemberImportStagingRows.AddRange(rows);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<ImportBatch?> GetTrackedByIdAsync(Guid importBatchId, CancellationToken cancellationToken)
    {
        return dbContext.ImportBatches.SingleOrDefaultAsync(batch => batch.Id == importBatchId, cancellationToken);
    }

    public async Task<IReadOnlyList<MemberImportStaging>> GetTrackedRowsByBatchIdAsync(Guid importBatchId, CancellationToken cancellationToken)
    {
        return await dbContext.MemberImportStagingRows
            .Where(row => row.ImportBatchId == importBatchId)
            .OrderBy(row => row.RowNumber)
            .ToListAsync(cancellationToken);
    }

    public Task AddValidationErrorsAsync(IReadOnlyCollection<ImportValidationError> errors, CancellationToken cancellationToken)
    {
        dbContext.ImportValidationErrors.AddRange(errors);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
