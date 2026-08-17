namespace Bimss.Application.Membership;

public interface IImportBatchQueryService
{
    Task<IReadOnlyList<ImportBatchSummary>> ListAsync(CancellationToken cancellationToken);

    Task<ImportBatchDetail?> GetByIdAsync(Guid importBatchId, CancellationToken cancellationToken);

    Task<IReadOnlyList<MemberImportStagingRowSummary>> ListRowsByBatchIdAsync(Guid importBatchId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ImportValidationErrorSummary>> ListErrorsByBatchIdAsync(Guid importBatchId, CancellationToken cancellationToken);
}
