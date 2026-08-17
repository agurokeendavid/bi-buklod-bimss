namespace Bimss.Domain.Membership;

// One field-level diff within a MemberUpdateRequest. internal constructor:
// only MemberUpdateRequest creates one, same pattern as
// MemberStatusHistory — keeps the child rows consistent with an actual
// submitted request rather than freestanding.
public sealed class MemberUpdateRequestChange
{
    internal MemberUpdateRequestChange(Guid id, Guid memberUpdateRequestId, string fieldName, string? oldValue, string? newValue)
    {
        if (memberUpdateRequestId == Guid.Empty)
        {
            throw new ArgumentException("Member update request is required.", nameof(memberUpdateRequestId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);

        Id = id;
        MemberUpdateRequestId = memberUpdateRequestId;
        FieldName = fieldName;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public Guid Id { get; private set; }

    public Guid MemberUpdateRequestId { get; private set; }

    public string FieldName { get; private set; } = string.Empty;

    public string? OldValue { get; private set; }

    public string? NewValue { get; private set; }
}
