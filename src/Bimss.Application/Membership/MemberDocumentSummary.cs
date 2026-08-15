namespace Bimss.Application.Membership;

public sealed record MemberDocumentSummary(
    Guid Id,
    string DocumentType,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    DateTimeOffset UploadedAtUtc,
    Guid? UploadedByUserId);
