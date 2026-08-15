using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberAddressTests
{
    [Fact]
    public void Constructor_Succeeds_WithCoreFields()
    {
        var id = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var address = new MemberAddress(id, memberId, MemberAddressType.Present, "123 Rizal St., Manila");

        Assert.Equal(id, address.Id);
        Assert.Equal(memberId, address.MemberId);
        Assert.Equal(MemberAddressType.Present, address.AddressType);
        Assert.Equal("123 Rizal St., Manila", address.AddressLine);
    }

    [Fact]
    public void Constructor_Throws_WhenMemberIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(
            () => new MemberAddress(Guid.NewGuid(), Guid.Empty, MemberAddressType.Present, "123 Rizal St., Manila"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenAddressLineIsMissing(string? addressLine)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => new MemberAddress(Guid.NewGuid(), Guid.NewGuid(), MemberAddressType.Permanent, addressLine!));
    }

    [Fact]
    public void UpdateAddressLine_UpdatesAddressLine()
    {
        var address = new MemberAddress(Guid.NewGuid(), Guid.NewGuid(), MemberAddressType.Permanent, "Old address");

        address.UpdateAddressLine("New address");

        Assert.Equal("New address", address.AddressLine);
    }

    [Fact]
    public void UpdateAddressLine_Throws_WhenBlank()
    {
        var address = new MemberAddress(Guid.NewGuid(), Guid.NewGuid(), MemberAddressType.Permanent, "Old address");

        Assert.ThrowsAny<ArgumentException>(() => address.UpdateAddressLine(" "));
    }
}
