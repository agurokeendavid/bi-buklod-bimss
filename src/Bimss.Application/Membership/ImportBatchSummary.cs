using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

// Projection for list/grid display (BIMSS-038's import batch admin
// screen) — never expose the ImportBatch EF entity directly.
public sealed record ImportBatchSummary(
    Guid Id,
    string FileName,
    ImportBatchStatus Status,
    int? RowCount,
    DateTimeOffset UploadedAtUtc,
    Guid UploadedByUserId);
