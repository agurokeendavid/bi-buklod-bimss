using Bimss.Domain.Membership.ReferenceData;

namespace Bimss.Infrastructure.Membership.ReferenceData;

public sealed class RelationshipTypeConfiguration : ReferenceDataItemConfiguration<RelationshipType>
{
    protected override string TableName => "RelationshipTypes";
}
