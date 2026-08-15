using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

public sealed record MemberStatusHistoryEntry(
    Guid Id,
    MemberStatus? FromStatus,
    MemberStatus ToStatus,
    Guid? ReasonId,
    Guid? ActorUserId,
    DateTimeOffset OccurredAtUtc,
    string? Remarks);
