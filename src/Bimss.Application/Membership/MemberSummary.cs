using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

// Projection for list/grid display (e.g. the future BIMSS-027 admin grid) —
// never expose the Member/MemberEmployment EF entities directly (AGENTS.md's
// data access rules).
public sealed record MemberSummary(
    Guid Id,
    string LastName,
    string FirstName,
    string? MiddleName,
    MemberStatus Status,
    string? EmployeeNumber);
