using Bimss.Domain.Membership.ReferenceData;

namespace Bimss.Infrastructure.Membership.ReferenceData;

public sealed class EducationalAttainmentConfiguration : ReferenceDataItemConfiguration<EducationalAttainment>
{
    protected override string TableName => "EducationalAttainments";
}
