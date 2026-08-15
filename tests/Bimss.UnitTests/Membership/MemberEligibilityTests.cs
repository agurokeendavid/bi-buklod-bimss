using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberEligibilityTests
{
    [Fact]
    public void Constructor_Succeeds_WithCoreFields()
    {
        var id = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var eligibilityTypeId = Guid.NewGuid();

        var eligibility = new MemberEligibility(id, memberId, eligibilityTypeId, "License No. 0123456");

        Assert.Equal(id, eligibility.Id);
        Assert.Equal(memberId, eligibility.MemberId);
        Assert.Equal(eligibilityTypeId, eligibility.EligibilityTypeId);
        Assert.Equal("License No. 0123456", eligibility.Details);
    }

    [Fact]
    public void Constructor_Succeeds_WithNullDetails()
    {
        var eligibility = new MemberEligibility(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), details: null);

        Assert.Null(eligibility.Details);
    }

    [Fact]
    public void Constructor_Throws_WhenMemberIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(
            () => new MemberEligibility(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), "License No. 0123456"));
    }

    [Fact]
    public void Constructor_Throws_WhenEligibilityTypeIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(
            () => new MemberEligibility(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, "License No. 0123456"));
    }

    [Fact]
    public void UpdateDetails_UpdatesDetails()
    {
        var eligibility = new MemberEligibility(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Old details");

        eligibility.UpdateDetails("New details");

        Assert.Equal("New details", eligibility.Details);
    }
}
