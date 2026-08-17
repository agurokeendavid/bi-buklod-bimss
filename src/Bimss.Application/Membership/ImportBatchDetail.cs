using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

public sealed record ImportBatchDetail(
    Guid Id,
    string FileName,
    ImportBatchStatus Status,
    int? RowCount,
    DateTimeOffset UploadedAtUtc,
    Guid UploadedByUserId,
    DateTimeOffset? StagedAtUtc,
    DateTimeOffset? ValidatedAtUtc,
    DateTimeOffset? PromotedAtUtc,
    DateTimeOffset? CancelledAtUtc,
    string? Remarks);
