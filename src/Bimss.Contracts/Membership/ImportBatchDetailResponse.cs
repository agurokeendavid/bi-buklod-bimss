namespace Bimss.Contracts.Membership;

public class ImportBatchDetailResponse
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int? RowCount { get; set; }

    public DateTimeOffset UploadedAtUtc { get; set; }

    public Guid UploadedByUserId { get; set; }

    public DateTimeOffset? StagedAtUtc { get; set; }

    public DateTimeOffset? ValidatedAtUtc { get; set; }

    public DateTimeOffset? PromotedAtUtc { get; set; }

    public DateTimeOffset? CancelledAtUtc { get; set; }

    public string? Remarks { get; set; }
}
