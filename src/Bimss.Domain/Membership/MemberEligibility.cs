namespace Bimss.Domain.Membership;

// A member can hold more than one eligibility (e.g. Civil Service
// Professional and a PRC license), so this is a child collection, not a
// 1:1 record like MemberEducation.
public sealed class MemberEligibility
{
    public MemberEligibility(Guid id, Guid memberId, Guid eligibilityTypeId, string? details)
    {
        if (memberId == Guid.Empty)
        {
            throw new ArgumentException("Member is required.", nameof(memberId));
        }

        if (eligibilityTypeId == Guid.Empty)
        {
            throw new ArgumentException("Eligibility type is required.", nameof(eligibilityTypeId));
        }

        Id = id;
        MemberId = memberId;
        EligibilityTypeId = eligibilityTypeId;
        Details = details;
    }

    public Guid Id { get; private set; }

    public Guid MemberId { get; private set; }

    public Guid EligibilityTypeId { get; private set; }

    public string? Details { get; private set; }

    public void UpdateDetails(string? details)
    {
        Details = details;
    }
}
