using Bimss.Domain.Exceptions;

namespace Bimss.Domain.Membership;

// Implements docs/DOMAIN_WORKFLOWS.md's "Member profile update" workflow:
// "Member edits permitted fields -> Submit Update Request -> Pending
// Review -> Membership Officer reviews differences -> Approve / Reject ->
// Approved changes applied -> History/audit recorded." This entity models
// submission and the review decision; actually applying an approved
// change to Member/MemberEmployment is the reviewing service's job
// (BIMSS-043), not this entity's — it only records what was requested and
// what was decided.
public sealed class MemberUpdateRequest
{
    private readonly List<MemberUpdateRequestChange> _changes = [];

    public MemberUpdateRequest(
        Guid id,
        Guid memberId,
        Guid submittedByUserId,
        DateTimeOffset submittedAtUtc,
        IReadOnlyCollection<MemberUpdateRequestChangeInput> changes)
    {
        if (memberId == Guid.Empty)
        {
            throw new ArgumentException("Member is required.", nameof(memberId));
        }

        if (submittedByUserId == Guid.Empty)
        {
            throw new ArgumentException("Submitting user is required.", nameof(submittedByUserId));
        }

        ArgumentNullException.ThrowIfNull(changes);
        if (changes.Count == 0)
        {
            throw new ArgumentException("At least one field change is required.", nameof(changes));
        }

        Id = id;
        MemberId = memberId;
        SubmittedByUserId = submittedByUserId;
        SubmittedAtUtc = submittedAtUtc;
        Status = MemberUpdateRequestStatus.Pending;

        foreach (var change in changes)
        {
            _changes.Add(new MemberUpdateRequestChange(Guid.NewGuid(), Id, change.FieldName, change.OldValue, change.NewValue));
        }
    }

    // EF Core materialization constructor: binds exactly the mapped scalar
    // properties, same reasoning as Member's private constructor.
    private MemberUpdateRequest(
        Guid id,
        Guid memberId,
        Guid submittedByUserId,
        DateTimeOffset submittedAtUtc,
        MemberUpdateRequestStatus status,
        Guid? reviewedByUserId,
        DateTimeOffset? reviewedAtUtc,
        string? reviewRemarks)
    {
        Id = id;
        MemberId = memberId;
        SubmittedByUserId = submittedByUserId;
        SubmittedAtUtc = submittedAtUtc;
        Status = status;
        ReviewedByUserId = reviewedByUserId;
        ReviewedAtUtc = reviewedAtUtc;
        ReviewRemarks = reviewRemarks;
    }

    public Guid Id { get; private set; }

    public Guid MemberId { get; private set; }

    public Guid SubmittedByUserId { get; private set; }

    public DateTimeOffset SubmittedAtUtc { get; private set; }

    public MemberUpdateRequestStatus Status { get; private set; }

    public Guid? ReviewedByUserId { get; private set; }

    public DateTimeOffset? ReviewedAtUtc { get; private set; }

    public string? ReviewRemarks { get; private set; }

    public IReadOnlyCollection<MemberUpdateRequestChange> Changes => _changes;

    public void Approve(Guid actorUserId, DateTimeOffset occurredAtUtc, string? remarks = null)
    {
        if (Status != MemberUpdateRequestStatus.Pending)
        {
            throw new ConflictException($"Cannot approve an update request with status '{Status}'.");
        }

        if (actorUserId == Guid.Empty)
        {
            throw new ArgumentException("Reviewing user is required.", nameof(actorUserId));
        }

        Status = MemberUpdateRequestStatus.Approved;
        ReviewedByUserId = actorUserId;
        ReviewedAtUtc = occurredAtUtc;
        ReviewRemarks = remarks;
    }

    // Remarks are mandatory on rejection (docs/design/BIMSS-UI-SPEC.md's
    // business rule: "Return and Deny require remarks") — the member needs
    // to know why, unlike Approve where the outcome speaks for itself.
    public void Reject(Guid actorUserId, DateTimeOffset occurredAtUtc, string remarks)
    {
        if (Status != MemberUpdateRequestStatus.Pending)
        {
            throw new ConflictException($"Cannot reject an update request with status '{Status}'.");
        }

        if (actorUserId == Guid.Empty)
        {
            throw new ArgumentException("Reviewing user is required.", nameof(actorUserId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(remarks);

        Status = MemberUpdateRequestStatus.Rejected;
        ReviewedByUserId = actorUserId;
        ReviewedAtUtc = occurredAtUtc;
        ReviewRemarks = remarks;
    }
}
