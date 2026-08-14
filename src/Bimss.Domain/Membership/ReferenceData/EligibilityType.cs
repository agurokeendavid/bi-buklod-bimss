namespace Bimss.Domain.Membership.ReferenceData;

public sealed class EligibilityType : ReferenceDataItem
{
    public EligibilityType(Guid id, string code, string name)
        : base(id, code, name)
    {
    }
}
