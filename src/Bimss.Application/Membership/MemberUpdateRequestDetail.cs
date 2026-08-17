using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

public sealed record MemberUpdateRequestDetail(
    Guid Id,
    Guid MemberId,
    string MemberLastName,
    string MemberFirstName,
    Guid SubmittedByUserId,
    DateTimeOffset SubmittedAtUtc,
    MemberUpdateRequestStatus Status,
    Guid? ReviewedByUserId,
    DateTimeOffset? ReviewedAtUtc,
    string? ReviewRemarks,
    IReadOnlyList<MemberUpdateRequestChangeSummary> Changes);
