namespace Bimss.Contracts.Membership;

public class MemberDocumentSummaryResponse
{
    public Guid Id { get; set; }

    public string DocumentType { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public DateTimeOffset UploadedAtUtc { get; set; }

    public Guid? UploadedByUserId { get; set; }
}
