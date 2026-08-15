namespace Bimss.Domain.Membership;

public sealed class MemberAddress
{
    public MemberAddress(Guid id, Guid memberId, MemberAddressType addressType, string addressLine)
    {
        if (memberId == Guid.Empty)
        {
            throw new ArgumentException("Member is required.", nameof(memberId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(addressLine);

        Id = id;
        MemberId = memberId;
        AddressType = addressType;
        AddressLine = addressLine;
    }

    public Guid Id { get; private set; }

    public Guid MemberId { get; private set; }

    public MemberAddressType AddressType { get; private set; }

    public string AddressLine { get; private set; } = string.Empty;

    // Address is unstructured text for Phase 1, per docs/DATA_DICTIONARY.md
    // ("nvarchar initially; later structured address if needed").
    public void UpdateAddressLine(string addressLine)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(addressLine);
        AddressLine = addressLine;
    }
}
