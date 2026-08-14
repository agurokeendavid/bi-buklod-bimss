using Bimss.Domain.Membership.ReferenceData;

namespace Bimss.Infrastructure.Membership.ReferenceData;

public sealed class OfficeUnitConfiguration : ReferenceDataItemConfiguration<OfficeUnit>
{
    protected override string TableName => "OfficeUnits";
}
