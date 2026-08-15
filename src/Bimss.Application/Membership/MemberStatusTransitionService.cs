using Bimss.Application.Auditing;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

// Wraps Member's Verify/Deactivate/Reactivate domain methods with loading,
// persistence, and the audit trail docs/ARCHITECTURE.md requires for member
// verification. Authorization (Permission.Membership.Verify/Manage) is
// enforced by the future controller that calls this service, not here — see
// BIMSS-022's MemberCreationService for the same convention.
public sealed class MemberStatusTransitionService(
    IMemberRepository memberRepository, IAuditLogger auditLogger, TimeProvider timeProvider)
{
    public async Task VerifyAsync(
        Guid memberId, Guid? actorUserId, string? remarks, CancellationToken cancellationToken = default)
    {
        var member = await LoadMemberAsync(memberId, cancellationToken);

        // Proof of employment is mandatory before verification (confirmed
        // with Buklod, docs/DATA_DICTIONARY.md). Checked here rather than on
        // Member itself — it requires querying MemberDocument, a different
        // table Member has no navigation property to (BIMSS-032).
        if (!await memberRepository.HasAnyDocumentAsync(memberId, cancellationToken))
        {
            throw new ConflictException("Cannot verify a member with no uploaded documents. Upload proof of employment first.");
        }

        member.Verify(actorUserId, timeProvider.GetUtcNow(), remarks);

        await memberRepository.SaveChangesAsync(cancellationToken);
        await LogAsync(actorUserId, "Member.Verify", memberId, remarks, cancellationToken);
    }

    public async Task DeactivateAsync(
        Guid memberId, Guid reasonId, Guid? actorUserId, string? remarks, CancellationToken cancellationToken = default)
    {
        var member = await LoadMemberAsync(memberId, cancellationToken);

        member.Deactivate(actorUserId, reasonId, timeProvider.GetUtcNow(), remarks);

        await memberRepository.SaveChangesAsync(cancellationToken);
        await LogAsync(actorUserId, "Member.Deactivate", memberId, remarks, cancellationToken);
    }

    public async Task ReactivateAsync(
        Guid memberId, Guid? actorUserId, string? remarks, CancellationToken cancellationToken = default)
    {
        var member = await LoadMemberAsync(memberId, cancellationToken);

        member.Reactivate(actorUserId, timeProvider.GetUtcNow(), remarks);

        await memberRepository.SaveChangesAsync(cancellationToken);
        await LogAsync(actorUserId, "Member.Reactivate", memberId, remarks, cancellationToken);
    }

    private async Task<Member> LoadMemberAsync(Guid memberId, CancellationToken cancellationToken)
    {
        return await memberRepository.GetTrackedByIdAsync(memberId, cancellationToken)
            ?? throw new NotFoundException("Member", memberId);
    }

    private Task LogAsync(Guid? actorUserId, string action, Guid memberId, string? remarks, CancellationToken cancellationToken)
    {
        return auditLogger.LogAsync(
            new AuditEntry(actorUserId, action, "Member", memberId.ToString(), AuditResult.Success, remarks),
            cancellationToken);
    }
}
