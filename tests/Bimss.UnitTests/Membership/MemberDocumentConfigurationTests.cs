using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.UnitTests.Membership;

public class MemberDocumentConfigurationTests
{
    [Fact]
    public void MemberDocument_IsConfigured_WithExpectedTableAndConstraints()
    {
        using var dbContext = CreateDbContext();

        var entityType = dbContext.Model.FindEntityType(typeof(MemberDocument));
        Assert.NotNull(entityType);
        Assert.Equal("MemberDocuments", entityType.GetTableName());

        var storageKeyProperty = entityType.FindProperty(nameof(MemberDocument.StorageKey));
        Assert.NotNull(storageKeyProperty);
        Assert.False(storageKeyProperty.IsNullable);
        Assert.Equal(200, storageKeyProperty.GetMaxLength());

        var originalFileNameProperty = entityType.FindProperty(nameof(MemberDocument.OriginalFileName));
        Assert.NotNull(originalFileNameProperty);
        Assert.False(originalFileNameProperty.IsNullable);
        Assert.Equal(260, originalFileNameProperty.GetMaxLength());

        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(MemberDocument.MemberId)));

        Assert.Contains(
            entityType.GetIndexes(),
            index => index.IsUnique && index.Properties.Any(p => p.Name == nameof(MemberDocument.StorageKey)));

        // A member can upload more than one document, so MemberId must not be unique.
        Assert.DoesNotContain(
            entityType.GetIndexes(),
            index => index.IsUnique && index.Properties.Any(p => p.Name == nameof(MemberDocument.MemberId)));
    }

    private static BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        return new BimssDbContext(optionsBuilder.Options);
    }
}
