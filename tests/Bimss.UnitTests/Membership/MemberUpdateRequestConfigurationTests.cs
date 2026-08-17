using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.UnitTests.Membership;

public class MemberUpdateRequestConfigurationTests
{
    [Fact]
    public void MemberUpdateRequest_IsConfigured_WithExpectedTableAndConstraints()
    {
        using var dbContext = CreateDbContext();

        var entityType = dbContext.Model.FindEntityType(typeof(MemberUpdateRequest));
        Assert.NotNull(entityType);
        Assert.Equal("MemberUpdateRequests", entityType.GetTableName());

        var statusProperty = entityType.FindProperty(nameof(MemberUpdateRequest.Status));
        Assert.NotNull(statusProperty);
        Assert.False(statusProperty.IsNullable);

        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.PrincipalEntityType.ClrType == typeof(Member)
                && fk.Properties.Any(p => p.Name == nameof(MemberUpdateRequest.MemberId)));

        Assert.Contains(
            entityType.GetIndexes(),
            index => index.Properties.Any(p => p.Name == nameof(MemberUpdateRequest.SubmittedByUserId)));
        Assert.Contains(
            entityType.GetIndexes(),
            index => index.Properties.Any(p => p.Name == nameof(MemberUpdateRequest.ReviewedByUserId)));
    }

    [Fact]
    public void MemberUpdateRequestChange_IsConfigured_WithExpectedTableAndConstraints()
    {
        using var dbContext = CreateDbContext();

        var entityType = dbContext.Model.FindEntityType(typeof(MemberUpdateRequestChange));
        Assert.NotNull(entityType);
        Assert.Equal("MemberUpdateRequestChanges", entityType.GetTableName());

        var fieldNameProperty = entityType.FindProperty(nameof(MemberUpdateRequestChange.FieldName));
        Assert.NotNull(fieldNameProperty);
        Assert.False(fieldNameProperty.IsNullable);
        Assert.Equal(100, fieldNameProperty.GetMaxLength());

        var oldValueProperty = entityType.FindProperty(nameof(MemberUpdateRequestChange.OldValue));
        Assert.NotNull(oldValueProperty);
        Assert.True(oldValueProperty.IsNullable);
        Assert.Null(oldValueProperty.GetMaxLength());

        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.PrincipalEntityType.ClrType == typeof(MemberUpdateRequest)
                && fk.Properties.Any(p => p.Name == nameof(MemberUpdateRequestChange.MemberUpdateRequestId)));
    }

    private static BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        return new BimssDbContext(optionsBuilder.Options);
    }
}
