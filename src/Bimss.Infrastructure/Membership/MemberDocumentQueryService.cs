using Bimss.Application.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.Infrastructure.Membership;

public sealed class MemberDocumentQueryService(BimssDbContext dbContext) : IMemberDocumentQueryService
{
    public async Task<IReadOnlyList<MemberDocumentSummary>> ListByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
    {
        return await dbContext.MemberDocuments
            .AsNoTracking()
            .Where(document => document.MemberId == memberId)
            .OrderByDescending(document => document.UploadedAtUtc)
            .Select(document => new MemberDocumentSummary(
                document.Id,
                document.DocumentType,
                document.OriginalFileName,
                document.ContentType,
                document.FileSizeBytes,
                document.UploadedAtUtc,
                document.UploadedByUserId))
            .ToListAsync(cancellationToken);
    }

    public Task<MemberDocumentDownload?> GetForDownloadAsync(Guid memberId, Guid documentId, CancellationToken cancellationToken)
    {
        return dbContext.MemberDocuments
            .AsNoTracking()
            .Where(document => document.MemberId == memberId && document.Id == documentId)
            .Select(document => new MemberDocumentDownload(
                document.Id, document.StorageKey, document.ContentType, document.OriginalFileName))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
