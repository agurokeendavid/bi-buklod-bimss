using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.UnitTests.Membership;

public class MemberEmploymentConfigurationTests
{
    [Fact]
    public void MemberEmployment_IsConfigured_WithExpectedTableAndConstraints()
    {
        using var dbContext = CreateDbContext();

        var entityType = dbContext.Model.FindEntityType(typeof(MemberEmployment));
        Assert.NotNull(entityType);
        Assert.Equal("MemberEmployments", entityType.GetTableName());

        var employeeNumberProperty = entityType.FindProperty(nameof(MemberEmployment.EmployeeNumber));
        Assert.NotNull(employeeNumberProperty);
        Assert.False(employeeNumberProperty.IsNullable);
        Assert.Equal(50, employeeNumberProperty.GetMaxLength());

        var positionProperty = entityType.FindProperty(nameof(MemberEmployment.PositionDesignation));
        Assert.NotNull(positionProperty);
        Assert.False(positionProperty.IsNullable);
        Assert.Equal(200, positionProperty.GetMaxLength());

        var permanentAppointmentDateProperty = entityType.FindProperty(nameof(MemberEmployment.PermanentAppointmentDate));
        Assert.NotNull(permanentAppointmentDateProperty);
        Assert.True(permanentAppointmentDateProperty.IsNullable);

        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(MemberEmployment.MemberId)));
        Assert.Contains(
            entityType.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(MemberEmployment.OfficeUnitId)));

        Assert.Contains(
            entityType.GetIndexes(),
            index => index.IsUnique && index.Properties.Any(p => p.Name == nameof(MemberEmployment.EmployeeNumber)));
        Assert.Contains(
            entityType.GetIndexes(),
            index => index.IsUnique && index.Properties.Any(p => p.Name == nameof(MemberEmployment.MemberId)));
    }

    private static BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        return new BimssDbContext(optionsBuilder.Options);
    }
}
