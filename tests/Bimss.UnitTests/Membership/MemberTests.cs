using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberTests
{
    private static readonly DateTimeOffset OccurredAt = new(2026, 8, 14, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_Succeeds_WithCoreFields()
    {
        var id = Guid.NewGuid();
        var civilStatusId = Guid.NewGuid();
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var member = new Member(
            id, "Dela Cruz", "Juan", "Santos", suffixId: null, dateOfBirth, "Manila", civilStatusId, "Referred by a colleague", OccurredAt);

        Assert.Equal(id, member.Id);
        Assert.Equal("Dela Cruz", member.LastName);
        Assert.Equal("Juan", member.FirstName);
        Assert.Equal("Santos", member.MiddleName);
        Assert.Null(member.SuffixId);
        Assert.Equal(dateOfBirth, member.DateOfBirth);
        Assert.Equal("Manila", member.PlaceOfBirth);
        Assert.Equal(civilStatusId, member.CivilStatusId);
        Assert.Equal("Referred by a colleague", member.JoiningReason);
        Assert.Equal(MemberStatus.PendingVerification, member.Status);

        var initialHistory = Assert.Single(member.StatusHistory);
        Assert.Null(initialHistory.FromStatus);
        Assert.Equal(MemberStatus.PendingVerification, initialHistory.ToStatus);
        Assert.Null(initialHistory.ReasonId);
        Assert.Null(initialHistory.ActorUserId);
        Assert.Equal(OccurredAt, initialHistory.OccurredAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenLastNameIsMissing(string? lastName)
    {
        Assert.ThrowsAny<ArgumentException>(() => CreateMember(lastName: lastName!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenFirstNameIsMissing(string? firstName)
    {
        Assert.ThrowsAny<ArgumentException>(() => CreateMember(firstName: firstName!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenPlaceOfBirthIsMissing(string? placeOfBirth)
    {
        Assert.ThrowsAny<ArgumentException>(() => CreateMember(placeOfBirth: placeOfBirth!));
    }

    [Fact]
    public void Constructor_Throws_WhenCivilStatusIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => CreateMember(civilStatusId: Guid.Empty));
    }

    [Fact]
    public void Verify_TransitionsToActive_AndRecordsHistory()
    {
        var member = CreateMember();
        var actorUserId = Guid.NewGuid();

        member.Verify(actorUserId, OccurredAt, "Documents checked");

        Assert.Equal(MemberStatus.Active, member.Status);
        Assert.Equal(2, member.StatusHistory.Count);

        var verifyEntry = member.StatusHistory.Last();
        Assert.Equal(MemberStatus.PendingVerification, verifyEntry.FromStatus);
        Assert.Equal(MemberStatus.Active, verifyEntry.ToStatus);
        Assert.Null(verifyEntry.ReasonId);
        Assert.Equal(actorUserId, verifyEntry.ActorUserId);
        Assert.Equal("Documents checked", verifyEntry.Remarks);
    }

    [Fact]
    public void Verify_Throws_WhenAlreadyVerified()
    {
        var member = CreateMember();
        member.Verify(Guid.NewGuid(), OccurredAt);

        Assert.Throws<ConflictException>(() => member.Verify(Guid.NewGuid(), OccurredAt));
    }

    [Fact]
    public void Deactivate_TransitionsToInactive_AndRecordsReason()
    {
        var member = CreateMember();
        member.Verify(Guid.NewGuid(), OccurredAt);

        var actorUserId = Guid.NewGuid();
        var reasonId = Guid.NewGuid();
        member.Deactivate(actorUserId, reasonId, OccurredAt, "Resigned from BI");

        Assert.Equal(MemberStatus.Inactive, member.Status);

        var deactivateEntry = member.StatusHistory.Last();
        Assert.Equal(MemberStatus.Active, deactivateEntry.FromStatus);
        Assert.Equal(MemberStatus.Inactive, deactivateEntry.ToStatus);
        Assert.Equal(reasonId, deactivateEntry.ReasonId);
        Assert.Equal(actorUserId, deactivateEntry.ActorUserId);
    }

    [Fact]
    public void Deactivate_Throws_WhenNotActive()
    {
        var member = CreateMember();

        Assert.Throws<ConflictException>(() => member.Deactivate(Guid.NewGuid(), Guid.NewGuid(), OccurredAt));
    }

    [Fact]
    public void Deactivate_Throws_WhenReasonIdIsEmpty()
    {
        var member = CreateMember();
        member.Verify(Guid.NewGuid(), OccurredAt);

        Assert.Throws<ArgumentException>(() => member.Deactivate(Guid.NewGuid(), Guid.Empty, OccurredAt));
    }

    [Fact]
    public void Reactivate_TransitionsToActive()
    {
        var member = CreateMember();
        member.Verify(Guid.NewGuid(), OccurredAt);
        member.Deactivate(Guid.NewGuid(), Guid.NewGuid(), OccurredAt);

        var actorUserId = Guid.NewGuid();
        member.Reactivate(actorUserId, OccurredAt, "Rejoined");

        Assert.Equal(MemberStatus.Active, member.Status);

        var reactivateEntry = member.StatusHistory.Last();
        Assert.Equal(MemberStatus.Inactive, reactivateEntry.FromStatus);
        Assert.Equal(MemberStatus.Active, reactivateEntry.ToStatus);
        Assert.Null(reactivateEntry.ReasonId);
        Assert.Equal(actorUserId, reactivateEntry.ActorUserId);
    }

    [Fact]
    public void Reactivate_Throws_WhenNotInactive()
    {
        var member = CreateMember();

        Assert.Throws<ConflictException>(() => member.Reactivate(Guid.NewGuid(), OccurredAt));
    }

    private static Member CreateMember(
        string lastName = "Dela Cruz",
        string firstName = "Juan",
        string placeOfBirth = "Manila",
        Guid? civilStatusId = null)
    {
        return new Member(
            Guid.NewGuid(),
            lastName,
            firstName,
            middleName: null,
            suffixId: null,
            new DateOnly(1990, 1, 1),
            placeOfBirth,
            civilStatusId ?? Guid.NewGuid(),
            joiningReason: null,
            OccurredAt);
    }
}
