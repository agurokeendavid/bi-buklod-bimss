namespace Bimss.Application.Membership;

// Carries the storage key needed to stream a download — deliberately kept
// out of MemberDocumentSummary, which backs the list response and has no
// reason to expose the internal storage identifier over the wire.
public sealed record MemberDocumentDownload(Guid Id, string StorageKey, string ContentType, string OriginalFileName);
