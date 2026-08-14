using Bimss.Domain.Membership.ReferenceData;

namespace Bimss.Infrastructure.Membership.ReferenceData;

public sealed class CivilStatusConfiguration : ReferenceDataItemConfiguration<CivilStatus>
{
    protected override string TableName => "CivilStatuses";
}
