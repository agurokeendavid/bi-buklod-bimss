namespace Bimss.Domain.Membership;

public sealed class MemberFamilyInformation
{
    public MemberFamilyInformation(
        Guid id,
        Guid memberId,
        string? spouseFullName,
        string? fatherFullName,
        string? motherMaidenName,
        string? parentsPresentAddress)
    {
        if (memberId == Guid.Empty)
        {
            throw new ArgumentException("Member is required.", nameof(memberId));
        }

        Id = id;
        MemberId = memberId;
        SpouseFullName = spouseFullName;
        FatherFullName = fatherFullName;
        MotherMaidenName = motherMaidenName;
        ParentsPresentAddress = parentsPresentAddress;
    }

    public Guid Id { get; private set; }

    public Guid MemberId { get; private set; }

    public string? SpouseFullName { get; private set; }

    public string? FatherFullName { get; private set; }

    public string? MotherMaidenName { get; private set; }

    public string? ParentsPresentAddress { get; private set; }

    public void UpdateDetails(
        string? spouseFullName, string? fatherFullName, string? motherMaidenName, string? parentsPresentAddress)
    {
        SpouseFullName = spouseFullName;
        FatherFullName = fatherFullName;
        MotherMaidenName = motherMaidenName;
        ParentsPresentAddress = parentsPresentAddress;
    }
}
