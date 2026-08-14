using Bimss.Domain.Membership.ReferenceData;

namespace Bimss.Infrastructure.Membership.ReferenceData;

public sealed class SuffixConfiguration : ReferenceDataItemConfiguration<Suffix>
{
    protected override string TableName => "Suffixes";
}
