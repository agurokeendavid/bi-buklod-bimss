namespace Bimss.Application.Membership;

// Read-only projections (AGENTS.md: "Use projections for lists, grids,
// dashboards, and reports" / "Do not expose EF entities directly from API
// endpoints"). Kept separate from IMemberRepository, which is write-focused.
public interface IMemberQueryService
{
    Task<MemberDetail?> GetByIdAsync(Guid memberId, CancellationToken cancellationToken);

    Task<IReadOnlyList<MemberSummary>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<MemberStatusHistoryEntry>> ListStatusHistoryAsync(Guid memberId, CancellationToken cancellationToken);

    // BIMSS-040: member self-service "My Profile" — resolves the member
    // linked to a login account (ApplicationUser.MemberId), with reference
    // names already joined in server-side (a self-service caller has no
    // permission to hit the officer-facing ReferenceDataController, which
    // stays scoped to Permission.Membership.Manage).
    Task<MyProfileDetail?> GetMyProfileByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    // BIMSS-042: a lighter lookup than GetMyProfileByUserIdAsync for
    // callers that just need to resolve "which member" without the
    // resolved-reference-name projection (submitting an update request
    // compares against Member/MemberEmployment directly, not display names).
    Task<Guid?> GetMemberIdByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
