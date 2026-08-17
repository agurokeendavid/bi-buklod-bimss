namespace Bimss.Contracts.Membership;

public class ImportBatchSummaryResponse
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int? RowCount { get; set; }

    public DateTimeOffset UploadedAtUtc { get; set; }

    public Guid UploadedByUserId { get; set; }
}
