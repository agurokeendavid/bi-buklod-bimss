using Bimss.Domain.Membership.ReferenceData;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.UnitTests.Membership.ReferenceData;

public class ReferenceDataConfigurationTests
{
    public static TheoryData<Type, string> ReferenceDataTypesAndTableNames => new()
    {
        { typeof(CivilStatus), "CivilStatuses" },
        { typeof(Suffix), "Suffixes" },
        { typeof(OfficeUnit), "OfficeUnits" },
        { typeof(EducationalAttainment), "EducationalAttainments" },
        { typeof(EligibilityType), "EligibilityTypes" },
        { typeof(RelationshipType), "RelationshipTypes" },
        { typeof(MemberStatusReason), "MemberStatusReasons" },
    };

    [Theory]
    [MemberData(nameof(ReferenceDataTypesAndTableNames))]
    public void EntityIsConfigured_WithExpectedTableAndConstraints(Type entityType, string expectedTableName)
    {
        using var dbContext = CreateDbContext();

        var entityTypeModel = dbContext.Model.FindEntityType(entityType);
        Assert.NotNull(entityTypeModel);

        Assert.Equal(expectedTableName, entityTypeModel.GetTableName());

        var codeProperty = entityTypeModel.FindProperty(nameof(ReferenceDataItem.Code));
        Assert.NotNull(codeProperty);
        Assert.False(codeProperty.IsNullable);
        Assert.Equal(50, codeProperty.GetMaxLength());

        var nameProperty = entityTypeModel.FindProperty(nameof(ReferenceDataItem.Name));
        Assert.NotNull(nameProperty);
        Assert.False(nameProperty.IsNullable);
        Assert.Equal(200, nameProperty.GetMaxLength());

        Assert.Contains(
            entityTypeModel.GetIndexes(),
            index => index.IsUnique && index.Properties.Select(p => p.Name).SequenceEqual([nameof(ReferenceDataItem.Code)]));
    }

    private static BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        return new BimssDbContext(optionsBuilder.Options);
    }
}
