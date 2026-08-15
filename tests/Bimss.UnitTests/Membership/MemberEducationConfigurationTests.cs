using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.UnitTests.Membership;

public class MemberEducationConfigurationTests
{
    [Fact]
    public void MemberEducation_IsConfigured_WithExpectedTableAndConstraints()
    {
        using var dbContext = CreateDbContext();

        var entityType = dbContext.Model.FindEntityType(typeof(MemberEducation));
        Assert.NotNull(entityType);
        Assert.Equal("MemberEducations", entityType.GetTableName());

        var degreeCourseProperty = entityType.FindProperty(nameof(MemberEducation.DegreeCourse));
        Assert.NotNull(degreeCourseProperty);
        Assert.True(degreeCourseProperty.IsNullable);
        Assert.Equal(200, degreeCourseProperty.GetMaxLength());

        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(MemberEducation.MemberId)));
        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(MemberEducation.HighestAttainmentId)));

        Assert.Contains(
            entityType.GetIndexes(),
            index => index.IsUnique && index.Properties.Any(p => p.Name == nameof(MemberEducation.MemberId)));
    }

    private static BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        return new BimssDbContext(optionsBuilder.Options);
    }
}
