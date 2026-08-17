using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.UnitTests.Membership;

public class MemberImportStagingConfigurationTests
{
    [Fact]
    public void MemberImportStaging_IsConfigured_WithExpectedTableAndConstraints()
    {
        using var dbContext = CreateDbContext();

        var entityType = dbContext.Model.FindEntityType(typeof(MemberImportStaging));
        Assert.NotNull(entityType);
        Assert.Equal("MemberImportStaging", entityType.GetTableName());

        var rowNumberProperty = entityType.FindProperty(nameof(MemberImportStaging.RowNumber));
        Assert.NotNull(rowNumberProperty);
        Assert.False(rowNumberProperty.IsNullable);

        var lastNameProperty = entityType.FindProperty(nameof(MemberImportStaging.LastName));
        Assert.NotNull(lastNameProperty);
        Assert.True(lastNameProperty.IsNullable);
        Assert.Null(lastNameProperty.GetMaxLength());

        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.PrincipalEntityType.ClrType == typeof(ImportBatch)
                && fk.Properties.Any(p => p.Name == nameof(MemberImportStaging.ImportBatchId)));

        Assert.Equal(
            2,
            entityType.GetForeignKeys().Count(fk => fk.PrincipalEntityType.ClrType == typeof(Member)));

        Assert.Contains(
            entityType.GetIndexes(),
            index => index.IsUnique
                && index.Properties.Select(p => p.Name).SequenceEqual([nameof(MemberImportStaging.ImportBatchId), nameof(MemberImportStaging.RowNumber)]));

        Assert.Contains(
            entityType.GetIndexes(),
            index => index.IsUnique && index.Properties.Any(p => p.Name == nameof(MemberImportStaging.PromotedMemberId)));

        Assert.Contains(
            entityType.GetIndexes(),
            index => !index.IsUnique && index.Properties.Any(p => p.Name == nameof(MemberImportStaging.MatchedMemberId)));
    }

    private static BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        return new BimssDbContext(optionsBuilder.Options);
    }
}
