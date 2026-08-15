namespace Bimss.Application.Membership;

public interface IMemberDocumentQueryService
{
    Task<IReadOnlyList<MemberDocumentSummary>> ListByMemberIdAsync(Guid memberId, CancellationToken cancellationToken);

    Task<MemberDocumentDownload?> GetForDownloadAsync(Guid memberId, Guid documentId, CancellationToken cancellationToken);
}
