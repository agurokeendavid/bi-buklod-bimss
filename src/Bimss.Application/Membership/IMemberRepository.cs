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

    // Tracked load for mutation via MemberEmployment.UpdateDetails (BIMSS-030).
    // Separate from GetTrackedByIdAsync because MemberEmployment is its own
    // row/table, not a navigation property on Member.
    Task<MemberEmployment?> GetTrackedEmploymentByMemberIdAsync(Guid memberId, CancellationToken cancellationToken);

    // BIMSS-032: document upload/verification-gate support.
    Task<bool> ExistsAsync(Guid memberId, CancellationToken cancellationToken);

    Task AddDocumentAsync(MemberDocument document, CancellationToken cancellationToken);

    Task<bool> HasAnyDocumentAsync(Guid memberId, CancellationToken cancellationToken);

    // BIMSS-044: self-service direct edit of contact info (the one profile
    // area a member can change without officer review, per
    // docs/DATA_DICTIONARY.md's confirmed decision).
    Task<MemberContact?> GetTrackedContactByMemberIdAsync(Guid memberId, CancellationToken cancellationToken);

    Task AddContactAsync(MemberContact contact, CancellationToken cancellationToken);

    Task<IReadOnlyList<MemberAddress>> GetTrackedAddressesByMemberIdAsync(Guid memberId, CancellationToken cancellationToken);

    Task AddAddressAsync(MemberAddress address, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
