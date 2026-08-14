using Bimss.Domain.Exceptions;

namespace Bimss.Domain.Membership;

public sealed class Member
{
    private readonly List<MemberStatusHistory> _statusHistory = [];

    public Member(
        Guid id,
        string lastName,
        string firstName,
        string? middleName,
        Guid? suffixId,
        DateOnly dateOfBirth,
        string placeOfBirth,
        Guid civilStatusId,
        string? joiningReason,
        DateTimeOffset occurredAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(placeOfBirth);
        if (civilStatusId == Guid.Empty)
        {
            throw new ArgumentException("Civil status is required.", nameof(civilStatusId));
        }

        Id = id;
        LastName = lastName;
        FirstName = firstName;
        MiddleName = middleName;
        SuffixId = suffixId;
        DateOfBirth = dateOfBirth;
        PlaceOfBirth = placeOfBirth;
        CivilStatusId = civilStatusId;
        JoiningReason = joiningReason;
        Status = MemberStatus.PendingVerification;

        _statusHistory.Add(new MemberStatusHistory(
            Guid.NewGuid(), Id, fromStatus: null, Status, reasonId: null, actorUserId: null, occurredAtUtc, remarks: null));
    }

    // EF Core materialization constructor: binds exactly the mapped scalar
    // properties, with no extra parameter (like occurredAtUtc above) that
    // would prevent EF from finding a usable constructor for query results.
    private Member(
        Guid id,
        string lastName,
        string firstName,
        string? middleName,
        Guid? suffixId,
        DateOnly dateOfBirth,
        string placeOfBirth,
        Guid civilStatusId,
        string? joiningReason,
        MemberStatus status)
    {
        Id = id;
        LastName = lastName;
        FirstName = firstName;
        MiddleName = middleName;
        SuffixId = suffixId;
        DateOfBirth = dateOfBirth;
        PlaceOfBirth = placeOfBirth;
        CivilStatusId = civilStatusId;
        JoiningReason = joiningReason;
        Status = status;
    }

    public Guid Id { get; private set; }

    public string LastName { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;

    public string? MiddleName { get; private set; }

    public Guid? SuffixId { get; private set; }

    public DateOnly DateOfBirth { get; private set; }

    public string PlaceOfBirth { get; private set; } = string.Empty;

    public Guid CivilStatusId { get; private set; }

    public string? JoiningReason { get; private set; }

    public MemberStatus Status { get; private set; }

    public IReadOnlyCollection<MemberStatusHistory> StatusHistory => _statusHistory;

    public void Verify(Guid? actorUserId, DateTimeOffset occurredAtUtc, string? remarks = null)
    {
        if (Status != MemberStatus.PendingVerification)
        {
            throw new ConflictException($"Cannot verify a member with status '{Status}'.");
        }

        TransitionTo(MemberStatus.Active, reasonId: null, actorUserId, occurredAtUtc, remarks);
    }

    public void Deactivate(Guid? actorUserId, Guid reasonId, DateTimeOffset occurredAtUtc, string? remarks = null)
    {
        if (Status != MemberStatus.Active)
        {
            throw new ConflictException($"Cannot deactivate a member with status '{Status}'.");
        }

        if (reasonId == Guid.Empty)
        {
            throw new ArgumentException("A status reason is required to deactivate a member.", nameof(reasonId));
        }

        TransitionTo(MemberStatus.Inactive, reasonId, actorUserId, occurredAtUtc, remarks);
    }

    public void Reactivate(Guid? actorUserId, DateTimeOffset occurredAtUtc, string? remarks = null)
    {
        if (Status != MemberStatus.Inactive)
        {
            throw new ConflictException($"Cannot reactivate a member with status '{Status}'.");
        }

        TransitionTo(MemberStatus.Active, reasonId: null, actorUserId, occurredAtUtc, remarks);
    }

    private void TransitionTo(
        MemberStatus toStatus, Guid? reasonId, Guid? actorUserId, DateTimeOffset occurredAtUtc, string? remarks)
    {
        var fromStatus = Status;
        Status = toStatus;
        _statusHistory.Add(new MemberStatusHistory(
            Guid.NewGuid(), Id, fromStatus, toStatus, reasonId, actorUserId, occurredAtUtc, remarks));
    }
}
