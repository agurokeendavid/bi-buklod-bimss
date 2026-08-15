namespace Bimss.Contracts.Membership;

public class MemberStatusHistoryResponse
{
    public Guid Id { get; set; }

    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = string.Empty;

    public Guid? ReasonId { get; set; }

    public Guid? ActorUserId { get; set; }

    public DateTimeOffset OccurredAtUtc { get; set; }

    public string? Remarks { get; set; }
}
