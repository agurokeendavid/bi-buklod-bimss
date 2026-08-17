using Bimss.Application.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.Infrastructure.Membership;

public sealed class ImportBatchQueryService(BimssDbContext dbContext) : IImportBatchQueryService
{
    public async Task<IReadOnlyList<ImportBatchSummary>> ListAsync(CancellationToken cancellationToken)
    {
        return await dbContext.ImportBatches
            .AsNoTracking()
            .OrderByDescending(batch => batch.UploadedAtUtc)
            .Select(batch => new ImportBatchSummary(
                batch.Id, batch.FileName, batch.Status, batch.RowCount, batch.UploadedAtUtc, batch.UploadedByUserId))
            .ToListAsync(cancellationToken);
    }

    public Task<ImportBatchDetail?> GetByIdAsync(Guid importBatchId, CancellationToken cancellationToken)
    {
        return dbContext.ImportBatches
            .AsNoTracking()
            .Where(batch => batch.Id == importBatchId)
            .Select(batch => new ImportBatchDetail(
                batch.Id,
                batch.FileName,
                batch.Status,
                batch.RowCount,
                batch.UploadedAtUtc,
                batch.UploadedByUserId,
                batch.StagedAtUtc,
                batch.ValidatedAtUtc,
                batch.PromotedAtUtc,
                batch.CancelledAtUtc,
                batch.Remarks))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MemberImportStagingRowSummary>> ListRowsByBatchIdAsync(
        Guid importBatchId, CancellationToken cancellationToken)
    {
        return await dbContext.MemberImportStagingRows
            .AsNoTracking()
            .Where(row => row.ImportBatchId == importBatchId)
            .OrderBy(row => row.RowNumber)
            .Select(row => new MemberImportStagingRowSummary(
                row.Id,
                row.RowNumber,
                row.LastName,
                row.FirstName,
                row.EmployeeNumber,
                row.ValidationStatus,
                row.MatchStatus,
                row.MatchedMemberId,
                row.PromotedMemberId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ImportValidationErrorSummary>> ListErrorsByBatchIdAsync(
        Guid importBatchId, CancellationToken cancellationToken)
    {
        return await dbContext.ImportValidationErrors
            .AsNoTracking()
            .Where(error => error.ImportBatchId == importBatchId)
            .OrderBy(error => error.DetectedAtUtc)
            .Select(error => new ImportValidationErrorSummary(
                error.Id, error.MemberImportStagingId, error.FieldName, error.Severity, error.Message))
            .ToListAsync(cancellationToken);
    }
}
