using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberContactTests
{
    [Fact]
    public void Constructor_Succeeds_WithCoreFields()
    {
        var id = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var contact = new MemberContact(id, memberId, "(02) 8123-4567", "09171234567", "juan.delacruz@example.com");

        Assert.Equal(id, contact.Id);
        Assert.Equal(memberId, contact.MemberId);
        Assert.Equal("(02) 8123-4567", contact.Landline);
        Assert.Equal("09171234567", contact.MobileNumber);
        Assert.Equal("juan.delacruz@example.com", contact.Email);
    }

    [Fact]
    public void Constructor_Succeeds_WithNullLandline()
    {
        var contact = new MemberContact(Guid.NewGuid(), Guid.NewGuid(), landline: null, "09171234567", "juan@example.com");

        Assert.Null(contact.Landline);
    }

    [Fact]
    public void Constructor_Throws_WhenMemberIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(
            () => new MemberContact(Guid.NewGuid(), Guid.Empty, null, "09171234567", "juan@example.com"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenMobileNumberIsMissing(string? mobileNumber)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => new MemberContact(Guid.NewGuid(), Guid.NewGuid(), null, mobileNumber!, "juan@example.com"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenEmailIsMissing(string? email)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => new MemberContact(Guid.NewGuid(), Guid.NewGuid(), null, "09171234567", email!));
    }

    [Fact]
    public void UpdateDetails_UpdatesMutableFields()
    {
        var contact = new MemberContact(Guid.NewGuid(), Guid.NewGuid(), "(02) 8123-4567", "09171234567", "juan@example.com");

        contact.UpdateDetails(null, "09179876543", "juan.new@example.com");

        Assert.Null(contact.Landline);
        Assert.Equal("09179876543", contact.MobileNumber);
        Assert.Equal("juan.new@example.com", contact.Email);
    }

    [Fact]
    public void UpdateDetails_Throws_WhenMobileNumberIsMissing()
    {
        var contact = new MemberContact(Guid.NewGuid(), Guid.NewGuid(), null, "09171234567", "juan@example.com");

        Assert.ThrowsAny<ArgumentException>(() => contact.UpdateDetails(null, " ", "juan@example.com"));
    }
}
