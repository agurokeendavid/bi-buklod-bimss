using Bimss.Application.Auditing;
using Bimss.Domain.Auditing;

namespace Bimss.UnitTests.Auditing;

public class AuditEntryTests
{
    [Fact]
    public void Constructor_Succeeds_WithAllRequiredFieldsPresent()
    {
        var entry = new AuditEntry(
            actorUserId: Guid.NewGuid(),
            action: "Member.Verify",
            objectType: "Member",
            objectId: "12345",
            result: AuditResult.Success,
            remarks: "Verified against BI employment records.");

        Assert.Equal("Member.Verify", entry.Action);
        Assert.Equal(AuditResult.Success, entry.Result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenActionIsMissing(string? action)
    {
        Assert.ThrowsAny<ArgumentException>(() => new AuditEntry(
            actorUserId: null,
            action: action!,
            objectType: "Member",
            objectId: "1",
            result: AuditResult.Success));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenObjectTypeIsMissing(string? objectType)
    {
        Assert.ThrowsAny<ArgumentException>(() => new AuditEntry(
            actorUserId: null,
            action: "Member.Verify",
            objectType: objectType!,
            objectId: "1",
            result: AuditResult.Success));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenObjectIdIsMissing(string? objectId)
    {
        Assert.ThrowsAny<ArgumentException>(() => new AuditEntry(
            actorUserId: null,
            action: "Member.Verify",
            objectType: "Member",
            objectId: objectId!,
            result: AuditResult.Success));
    }
}
