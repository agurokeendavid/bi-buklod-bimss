using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

// Narrow, use-case-specific port — same reasoning as IMemberRepository/
// IImportBatchRepository (AGENTS.md: no generic repository abstraction).
// Grows incrementally: BIMSS-042 only needs to persist a freshly submitted
// request; BIMSS-043 (review) will add loading/decision methods.
public interface IMemberUpdateRequestRepository
{
    Task AddAsync(MemberUpdateRequest request, CancellationToken cancellationToken);

    // BIMSS-043: officer review.
    Task<MemberUpdateRequest?> GetTrackedByIdAsync(Guid requestId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
