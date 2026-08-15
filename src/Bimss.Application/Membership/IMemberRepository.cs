using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

// Narrow, use-case-specific port — not a generic repository abstraction
// (AGENTS.md: "Do not create a generic repository abstraction over EF Core
// unless there is a demonstrated need"). The need here is that
// Bimss.Application cannot reference Bimss.Infrastructure (BimssDbContext)
// per the enforced layering rule, so member creation and status transitions
// need a persistence seam scoped to exactly what they do.
public interface IMemberRepository
{
    Task<bool> EmployeeNumberExistsAsync(string employeeNumber, CancellationToken cancellationToken);

    Task AddAsync(Member member, MemberEmployment employment, CancellationToken cancellationToken);

    // Tracked load for mutation via the aggregate's own domain methods
    // (Verify/Deactivate/Reactivate), unlike IMemberQueryService's untracked
    // projections.
    Task<Member?> GetTrackedByIdAsync(Guid memberId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
