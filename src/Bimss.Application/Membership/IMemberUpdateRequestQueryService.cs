using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

public interface IMemberUpdateRequestQueryService
{
    Task<IReadOnlyList<MemberUpdateRequestSummary>> ListAsync(MemberUpdateRequestStatus? status, CancellationToken cancellationToken);

    Task<MemberUpdateRequestDetail?> GetByIdAsync(Guid requestId, CancellationToken cancellationToken);

    // BIMSS-045: member self-service status/history view — the same
    // summary projection as the officer queue's ListAsync, scoped to one
    // member instead of status, so a member sees only their own requests.
    Task<IReadOnlyList<MemberUpdateRequestSummary>> ListByMemberIdAsync(Guid memberId, CancellationToken cancellationToken);
}
