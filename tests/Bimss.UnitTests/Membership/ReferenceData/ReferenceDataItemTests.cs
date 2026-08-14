using Bimss.Domain.Membership.ReferenceData;

namespace Bimss.UnitTests.Membership.ReferenceData;

public class ReferenceDataItemTests
{
    [Fact]
    public void Constructor_Succeeds_WithCodeAndName()
    {
        var id = Guid.NewGuid();

        var civilStatus = new CivilStatus(id, "MARRIED", "Married");

        Assert.Equal(id, civilStatus.Id);
        Assert.Equal("MARRIED", civilStatus.Code);
        Assert.Equal("Married", civilStatus.Name);
        Assert.True(civilStatus.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenCodeIsMissing(string? code)
    {
        Assert.ThrowsAny<ArgumentException>(() => new CivilStatus(Guid.NewGuid(), code!, "Married"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenNameIsMissing(string? name)
    {
        Assert.ThrowsAny<ArgumentException>(() => new CivilStatus(Guid.NewGuid(), "MARRIED", name!));
    }

    [Fact]
    public void SetActive_UpdatesIsActive()
    {
        var civilStatus = new CivilStatus(Guid.NewGuid(), "MARRIED", "Married");

        civilStatus.SetActive(false);
        Assert.False(civilStatus.IsActive);

        civilStatus.SetActive(true);
        Assert.True(civilStatus.IsActive);
    }

    [Fact]
    public void ConcreteTypes_Construct_WithCodeAndName()
    {
        var id = Guid.NewGuid();

        Assert.Equal("JR", new Suffix(id, "JR", "Jr.").Code);
        Assert.Equal("HR", new OfficeUnit(id, "HR", "Human Resources").Code);
        Assert.Equal("BACHELOR", new EducationalAttainment(id, "BACHELOR", "Bachelor's Degree").Code);
        Assert.Equal("CS-PROF", new EligibilityType(id, "CS-PROF", "Civil Service Professional").Code);
        Assert.Equal("SPOUSE", new RelationshipType(id, "SPOUSE", "Spouse").Code);
        Assert.Equal("RESIGNED", new MemberStatusReason(id, "RESIGNED", "Resigned").Code);
    }
}
