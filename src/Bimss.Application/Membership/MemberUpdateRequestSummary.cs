using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

// Projection for the officer-facing review queue — includes the member's
// name so the list is meaningful without a second lookup per row.
public sealed record MemberUpdateRequestSummary(
    Guid Id,
    Guid MemberId,
    string MemberLastName,
    string MemberFirstName,
    Guid SubmittedByUserId,
    DateTimeOffset SubmittedAtUtc,
    MemberUpdateRequestStatus Status);
