using Bimss.Application.Auditing;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

public sealed class MemberDocumentUploadService(
    IMemberRepository memberRepository,
    IMemberDocumentStorage documentStorage,
    IAuditLogger auditLogger,
    TimeProvider timeProvider)
{
    public async Task<Guid> UploadAsync(
        Guid memberId,
        string documentType,
        string originalFileName,
        string contentType,
        Stream content,
        long fileSizeBytes,
        Guid? actorUserId,
        CancellationToken cancellationToken = default)
    {
        if (!await memberRepository.ExistsAsync(memberId, cancellationToken))
        {
            throw new NotFoundException("Member", memberId);
        }

        var storageKey = await documentStorage.SaveAsync(content, cancellationToken);

        var document = new MemberDocument(
            Guid.NewGuid(),
            memberId,
            documentType,
            originalFileName,
            contentType,
            storageKey,
            fileSizeBytes,
            timeProvider.GetUtcNow(),
            actorUserId);

        await memberRepository.AddDocumentAsync(document, cancellationToken);

        await auditLogger.LogAsync(
            new AuditEntry(actorUserId, "Member.UploadDocument", "Member", memberId.ToString(), AuditResult.Success),
            cancellationToken);

        return document.Id;
    }
}
