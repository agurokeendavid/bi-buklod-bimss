using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberChildTests
{
    [Fact]
    public void Constructor_Succeeds_WithCoreFields()
    {
        var id = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var dateOfBirth = new DateOnly(2015, 4, 10);

        var child = new MemberChild(id, memberId, "Maria Dela Cruz", dateOfBirth);

        Assert.Equal(id, child.Id);
        Assert.Equal(memberId, child.MemberId);
        Assert.Equal("Maria Dela Cruz", child.Name);
        Assert.Equal(dateOfBirth, child.DateOfBirth);
    }

    [Fact]
    public void Constructor_Throws_WhenMemberIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(
            () => new MemberChild(Guid.NewGuid(), Guid.Empty, "Maria Dela Cruz", new DateOnly(2015, 4, 10)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenNameIsMissing(string? name)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => new MemberChild(Guid.NewGuid(), Guid.NewGuid(), name!, new DateOnly(2015, 4, 10)));
    }

    [Fact]
    public void UpdateDetails_UpdatesMutableFields()
    {
        var child = new MemberChild(Guid.NewGuid(), Guid.NewGuid(), "Maria Dela Cruz", new DateOnly(2015, 4, 10));
        var newDateOfBirth = new DateOnly(2015, 4, 11);

        child.UpdateDetails("Maria D. Cruz", newDateOfBirth);

        Assert.Equal("Maria D. Cruz", child.Name);
        Assert.Equal(newDateOfBirth, child.DateOfBirth);
    }

    [Fact]
    public void UpdateDetails_Throws_WhenNameIsMissing()
    {
        var child = new MemberChild(Guid.NewGuid(), Guid.NewGuid(), "Maria Dela Cruz", new DateOnly(2015, 4, 10));

        Assert.ThrowsAny<ArgumentException>(() => child.UpdateDetails(" ", new DateOnly(2015, 4, 10)));
    }
}
