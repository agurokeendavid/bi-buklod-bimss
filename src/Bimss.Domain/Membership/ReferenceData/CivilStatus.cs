namespace Bimss.Domain.Membership.ReferenceData;

public sealed class CivilStatus : ReferenceDataItem
{
    public CivilStatus(Guid id, string code, string name)
        : base(id, code, name)
    {
    }
}
