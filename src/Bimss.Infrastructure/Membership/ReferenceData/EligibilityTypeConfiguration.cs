using Bimss.Domain.Membership.ReferenceData;

namespace Bimss.Infrastructure.Membership.ReferenceData;

public sealed class EligibilityTypeConfiguration : ReferenceDataItemConfiguration<EligibilityType>
{
    protected override string TableName => "EligibilityTypes";
}
