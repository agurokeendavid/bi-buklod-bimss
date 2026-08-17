using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.UnitTests.Membership;

public class ImportValidationErrorConfigurationTests
{
    [Fact]
    public void ImportValidationError_IsConfigured_WithExpectedTableAndConstraints()
    {
        using var dbContext = CreateDbContext();

        var entityType = dbContext.Model.FindEntityType(typeof(ImportValidationError));
        Assert.NotNull(entityType);
        Assert.Equal("ImportValidationErrors", entityType.GetTableName());

        var messageProperty = entityType.FindProperty(nameof(ImportValidationError.Message));
        Assert.NotNull(messageProperty);
        Assert.False(messageProperty.IsNullable);
        Assert.Equal(2000, messageProperty.GetMaxLength());

        var fieldNameProperty = entityType.FindProperty(nameof(ImportValidationError.FieldName));
        Assert.NotNull(fieldNameProperty);
        Assert.True(fieldNameProperty.IsNullable);

        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.PrincipalEntityType.ClrType == typeof(ImportBatch)
                && fk.Properties.Any(p => p.Name == nameof(ImportValidationError.ImportBatchId)));

        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.PrincipalEntityType.ClrType == typeof(MemberImportStaging)
                && fk.Properties.Any(p => p.Name == nameof(ImportValidationError.MemberImportStagingId)));
    }

    private static BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        return new BimssDbContext(optionsBuilder.Options);
    }
}
