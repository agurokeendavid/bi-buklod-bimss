namespace Bimss.Domain.Membership;

public sealed class MemberContact
{
    public MemberContact(Guid id, Guid memberId, string? landline, string mobileNumber, string email)
    {
        if (memberId == Guid.Empty)
        {
            throw new ArgumentException("Member is required.", nameof(memberId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(mobileNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        Id = id;
        MemberId = memberId;
        Landline = landline;
        MobileNumber = mobileNumber;
        Email = email;
    }

    public Guid Id { get; private set; }

    public Guid MemberId { get; private set; }

    public string? Landline { get; private set; }

    public string MobileNumber { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    // Contact info is the one profile area members can edit directly without
    // officer review (confirmed decision, docs/DATA_DICTIONARY.md).
    public void UpdateDetails(string? landline, string mobileNumber, string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mobileNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        Landline = landline;
        MobileNumber = mobileNumber;
        Email = email;
    }
}
