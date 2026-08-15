using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberPrivacyConsentTests
{
    private static readonly DateTimeOffset ConsentedAt = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_Succeeds_WithCoreFields()
    {
        var id = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var consent = new MemberPrivacyConsent(id, memberId, true, "2026-08-14", ConsentedAt, "Web Form");

        Assert.Equal(id, consent.Id);
        Assert.Equal(memberId, consent.MemberId);
        Assert.True(consent.ConsentGiven);
        Assert.Equal("2026-08-14", consent.NoticeVersion);
        Assert.Equal(ConsentedAt, consent.ConsentedAtUtc);
        Assert.Equal("Web Form", consent.Source);
    }

    [Fact]
    public void Constructor_Succeeds_WithConsentWithheld()
    {
        var consent = new MemberPrivacyConsent(Guid.NewGuid(), Guid.NewGuid(), false, "2026-08-14", ConsentedAt, "Web Form");

        Assert.False(consent.ConsentGiven);
    }

    [Fact]
    public void Constructor_Throws_WhenMemberIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(
            () => new MemberPrivacyConsent(Guid.NewGuid(), Guid.Empty, true, "2026-08-14", ConsentedAt, "Web Form"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenNoticeVersionIsMissing(string? noticeVersion)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => new MemberPrivacyConsent(Guid.NewGuid(), Guid.NewGuid(), true, noticeVersion!, ConsentedAt, "Web Form"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenSourceIsMissing(string? source)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => new MemberPrivacyConsent(Guid.NewGuid(), Guid.NewGuid(), true, "2026-08-14", ConsentedAt, source!));
    }
}
