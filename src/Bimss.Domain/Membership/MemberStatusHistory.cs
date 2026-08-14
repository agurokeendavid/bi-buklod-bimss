namespace Bimss.Domain.Membership;

public sealed class MemberStatusHistory
{
    internal MemberStatusHistory(
        Guid id,
        Guid memberId,
        MemberStatus? fromStatus,
        MemberStatus toStatus,
        Guid? reasonId,
        Guid? actorUserId,
        DateTimeOffset occurredAtUtc,
        string? remarks)
    {
        Id = id;
        MemberId = memberId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ReasonId = reasonId;
        ActorUserId = actorUserId;
        OccurredAtUtc = occurredAtUtc;
        Remarks = remarks;
    }

    public Guid Id { get; private set; }

    public Guid MemberId { get; private set; }

    public MemberStatus? FromStatus { get; private set; }

    public MemberStatus ToStatus { get; private set; }

    public Guid? ReasonId { get; private set; }

    public Guid? ActorUserId { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public string? Remarks { get; private set; }
}
