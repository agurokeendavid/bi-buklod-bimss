using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberEducationTests
{
    [Fact]
    public void Constructor_Succeeds_WithCoreFields()
    {
        var id = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var attainmentId = Guid.NewGuid();

        var education = new MemberEducation(id, memberId, attainmentId, "BS Criminology");

        Assert.Equal(id, education.Id);
        Assert.Equal(memberId, education.MemberId);
        Assert.Equal(attainmentId, education.HighestAttainmentId);
        Assert.Equal("BS Criminology", education.DegreeCourse);
    }

    [Fact]
    public void Constructor_Succeeds_WithNullDegreeCourse()
    {
        var education = new MemberEducation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), degreeCourse: null);

        Assert.Null(education.DegreeCourse);
    }

    [Fact]
    public void Constructor_Throws_WhenMemberIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(
            () => new MemberEducation(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), "BS Criminology"));
    }

    [Fact]
    public void Constructor_Throws_WhenHighestAttainmentIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(
            () => new MemberEducation(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, "BS Criminology"));
    }

    [Fact]
    public void UpdateDetails_UpdatesMutableFields()
    {
        var education = new MemberEducation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "BS Criminology");
        var newAttainmentId = Guid.NewGuid();

        education.UpdateDetails(newAttainmentId, "MA Public Administration");

        Assert.Equal(newAttainmentId, education.HighestAttainmentId);
        Assert.Equal("MA Public Administration", education.DegreeCourse);
    }

    [Fact]
    public void UpdateDetails_Throws_WhenHighestAttainmentIdIsEmpty()
    {
        var education = new MemberEducation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "BS Criminology");

        Assert.Throws<ArgumentException>(() => education.UpdateDetails(Guid.Empty, "MA Public Administration"));
    }
}
