using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberEmploymentTests
{
    [Fact]
    public void Constructor_Succeeds_WithCoreFields()
    {
        var id = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var officeUnitId = Guid.NewGuid();
        var appointmentDate = new DateOnly(2020, 6, 1);

        var employment = new MemberEmployment(id, memberId, "BI-00123", "Immigration Officer I", officeUnitId, appointmentDate);

        Assert.Equal(id, employment.Id);
        Assert.Equal(memberId, employment.MemberId);
        Assert.Equal("BI-00123", employment.EmployeeNumber);
        Assert.Equal("Immigration Officer I", employment.PositionDesignation);
        Assert.Equal(officeUnitId, employment.OfficeUnitId);
        Assert.Equal(appointmentDate, employment.PermanentAppointmentDate);
    }

    [Fact]
    public void Constructor_Succeeds_WithNullPermanentAppointmentDate()
    {
        var employment = new MemberEmployment(
            Guid.NewGuid(), Guid.NewGuid(), "BI-00123", "Immigration Officer I", Guid.NewGuid(), permanentAppointmentDate: null);

        Assert.Null(employment.PermanentAppointmentDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenEmployeeNumberIsMissing(string? employeeNumber)
    {
        Assert.ThrowsAny<ArgumentException>(() => CreateEmployment(employeeNumber: employeeNumber!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenPositionDesignationIsMissing(string? positionDesignation)
    {
        Assert.ThrowsAny<ArgumentException>(() => CreateEmployment(positionDesignation: positionDesignation!));
    }

    [Fact]
    public void Constructor_Throws_WhenMemberIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => CreateEmployment(memberId: Guid.Empty));
    }

    [Fact]
    public void Constructor_Throws_WhenOfficeUnitIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => CreateEmployment(officeUnitId: Guid.Empty));
    }

    [Fact]
    public void UpdateDetails_UpdatesMutableFields()
    {
        var employment = CreateEmployment();
        var newOfficeUnitId = Guid.NewGuid();
        var newDate = new DateOnly(2021, 3, 15);

        employment.UpdateDetails("Immigration Officer II", newOfficeUnitId, newDate);

        Assert.Equal("Immigration Officer II", employment.PositionDesignation);
        Assert.Equal(newOfficeUnitId, employment.OfficeUnitId);
        Assert.Equal(newDate, employment.PermanentAppointmentDate);
    }

    [Fact]
    public void UpdateDetails_Throws_WhenPositionDesignationIsMissing()
    {
        var employment = CreateEmployment();

        Assert.ThrowsAny<ArgumentException>(() => employment.UpdateDetails(" ", Guid.NewGuid(), null));
    }

    [Fact]
    public void UpdateDetails_Throws_WhenOfficeUnitIdIsEmpty()
    {
        var employment = CreateEmployment();

        Assert.Throws<ArgumentException>(() => employment.UpdateDetails("Immigration Officer II", Guid.Empty, null));
    }

    private static MemberEmployment CreateEmployment(
        string employeeNumber = "BI-00123",
        string positionDesignation = "Immigration Officer I",
        Guid? memberId = null,
        Guid? officeUnitId = null,
        DateOnly? permanentAppointmentDate = null)
    {
        return new MemberEmployment(
            Guid.NewGuid(),
            memberId ?? Guid.NewGuid(),
            employeeNumber,
            positionDesignation,
            officeUnitId ?? Guid.NewGuid(),
            permanentAppointmentDate ?? new DateOnly(2020, 6, 1));
    }
}
