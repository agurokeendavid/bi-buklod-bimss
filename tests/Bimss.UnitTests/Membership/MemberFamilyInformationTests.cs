using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberFamilyInformationTests
{
    [Fact]
    public void Constructor_Succeeds_WithCoreFields()
    {
        var id = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var family = new MemberFamilyInformation(
            id, memberId, "Maria Dela Cruz", "Pedro Dela Cruz", "Reyes", "12 Mabini St., Batangas");

        Assert.Equal(id, family.Id);
        Assert.Equal(memberId, family.MemberId);
        Assert.Equal("Maria Dela Cruz", family.SpouseFullName);
        Assert.Equal("Pedro Dela Cruz", family.FatherFullName);
        Assert.Equal("Reyes", family.MotherMaidenName);
        Assert.Equal("12 Mabini St., Batangas", family.ParentsPresentAddress);
    }

    [Fact]
    public void Constructor_Succeeds_WithAllNullableFieldsNull()
    {
        var family = new MemberFamilyInformation(Guid.NewGuid(), Guid.NewGuid(), null, null, null, null);

        Assert.Null(family.SpouseFullName);
        Assert.Null(family.FatherFullName);
        Assert.Null(family.MotherMaidenName);
        Assert.Null(family.ParentsPresentAddress);
    }

    [Fact]
    public void Constructor_Throws_WhenMemberIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(
            () => new MemberFamilyInformation(Guid.NewGuid(), Guid.Empty, null, null, null, null));
    }

    [Fact]
    public void UpdateDetails_UpdatesAllFields()
    {
        var family = new MemberFamilyInformation(Guid.NewGuid(), Guid.NewGuid(), null, null, null, null);

        family.UpdateDetails("Maria Dela Cruz", "Pedro Dela Cruz", "Reyes", "12 Mabini St., Batangas");

        Assert.Equal("Maria Dela Cruz", family.SpouseFullName);
        Assert.Equal("Pedro Dela Cruz", family.FatherFullName);
        Assert.Equal("Reyes", family.MotherMaidenName);
        Assert.Equal("12 Mabini St., Batangas", family.ParentsPresentAddress);
    }
}
