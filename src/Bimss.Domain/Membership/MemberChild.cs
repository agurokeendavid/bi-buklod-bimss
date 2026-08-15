namespace Bimss.Domain.Membership;

// Both Name and DateOfBirth are mandatory (confirmed with Buklod, 2026-08-14
// — birth date is not optional, unlike most other Membership child fields).
public sealed class MemberChild
{
    public MemberChild(Guid id, Guid memberId, string name, DateOnly dateOfBirth)
    {
        if (memberId == Guid.Empty)
        {
            throw new ArgumentException("Member is required.", nameof(memberId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        MemberId = memberId;
        Name = name;
        DateOfBirth = dateOfBirth;
    }

    public Guid Id { get; private set; }

    public Guid MemberId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateOnly DateOfBirth { get; private set; }

    public void UpdateDetails(string name, DateOnly dateOfBirth)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        DateOfBirth = dateOfBirth;
    }
}
