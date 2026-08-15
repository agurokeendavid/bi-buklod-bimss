namespace Bimss.Application.Membership;

// Read-only projections (AGENTS.md: "Use projections for lists, grids,
// dashboards, and reports" / "Do not expose EF entities directly from API
// endpoints"). Kept separate from IMemberRepository, which is write-focused.
public interface IMemberQueryService
{
    Task<MemberDetail?> GetByIdAsync(Guid memberId, CancellationToken cancellationToken);

    Task<IReadOnlyList<MemberSummary>> ListAsync(CancellationToken cancellationToken);
}
