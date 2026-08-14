namespace Bimss.Domain.Membership.ReferenceData;

public sealed class MemberStatusReason : ReferenceDataItem
{
    public MemberStatusReason(Guid id, string code, string name)
        : base(id, code, name)
    {
    }
}
