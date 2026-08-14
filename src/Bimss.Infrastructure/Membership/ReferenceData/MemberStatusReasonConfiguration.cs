using Bimss.Domain.Membership.ReferenceData;

namespace Bimss.Infrastructure.Membership.ReferenceData;

public sealed class MemberStatusReasonConfiguration : ReferenceDataItemConfiguration<MemberStatusReason>
{
    protected override string TableName => "MemberStatusReasons";
}
