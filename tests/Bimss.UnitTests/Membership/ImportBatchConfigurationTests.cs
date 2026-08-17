using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.UnitTests.Membership;

public class ImportBatchConfigurationTests
{
    [Fact]
    public void ImportBatch_IsConfigured_WithExpectedTableAndConstraints()
    {
        using var dbContext = CreateDbContext();

        var entityType = dbContext.Model.FindEntityType(typeof(ImportBatch));
        Assert.NotNull(entityType);
        Assert.Equal("ImportBatches", entityType.GetTableName());

        var fileNameProperty = entityType.FindProperty(nameof(ImportBatch.FileName));
        Assert.NotNull(fileNameProperty);
        Assert.False(fileNameProperty.IsNullable);
        Assert.Equal(260, fileNameProperty.GetMaxLength());

        var statusProperty = entityType.FindProperty(nameof(ImportBatch.Status));
        Assert.NotNull(statusProperty);
        Assert.False(statusProperty.IsNullable);

        var rowCountProperty = entityType.FindProperty(nameof(ImportBatch.RowCount));
        Assert.NotNull(rowCountProperty);
        Assert.True(rowCountProperty.IsNullable);

        Assert.Contains(
            entityType.GetIndexes(),
            index => index.Properties.Any(p => p.Name == nameof(ImportBatch.UploadedByUserId)));

        Assert.Empty(entityType.GetForeignKeys());
    }

    private static BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        return new BimssDbContext(optionsBuilder.Options);
    }
}
