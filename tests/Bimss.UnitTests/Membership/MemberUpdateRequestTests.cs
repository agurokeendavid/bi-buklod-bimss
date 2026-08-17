using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.UnitTests.Membership;

public class MemberUpdateRequestTests
{
    private static readonly DateTimeOffset OccurredAt = new(2026, 8, 17, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_Succeeds_WithChanges()
    {
        var id = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var submittedByUserId = Guid.NewGuid();
        var changes = new[] { new MemberUpdateRequestChangeInput("LastName", "Dela Cruz", "Dela Cruz-Santos") };

        var request = new MemberUpdateRequest(id, memberId, submittedByUserId, OccurredAt, changes);

        Assert.Equal(id, request.Id);
        Assert.Equal(memberId, request.MemberId);
        Assert.Equal(submittedByUserId, request.SubmittedByUserId);
        Assert.Equal(OccurredAt, request.SubmittedAtUtc);
        Assert.Equal(MemberUpdateRequestStatus.Pending, request.Status);
        Assert.Null(request.ReviewedByUserId);

        var change = Assert.Single(request.Changes);
        Assert.Equal(request.Id, change.MemberUpdateRequestId);
        Assert.Equal("LastName", change.FieldName);
        Assert.Equal("Dela Cruz", change.OldValue);
        Assert.Equal("Dela Cruz-Santos", change.NewValue);
    }

    [Fact]
    public void Constructor_Succeeds_WithMultipleChanges()
    {
        var changes = new[]
        {
            new MemberUpdateRequestChangeInput("LastName", "Dela Cruz", "Santos"),
            new MemberUpdateRequestChangeInput("CivilStatusId", "single-id", "married-id"),
        };

        var request = CreateRequest(changes);

        Assert.Equal(2, request.Changes.Count);
    }

    [Fact]
    public void Constructor_Throws_WhenMemberIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new MemberUpdateRequest(
            Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), OccurredAt, [new MemberUpdateRequestChangeInput("LastName", null, "Santos")]));
    }

    [Fact]
    public void Constructor_Throws_WhenSubmittedByUserIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new MemberUpdateRequest(
            Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, OccurredAt, [new MemberUpdateRequestChangeInput("LastName", null, "Santos")]));
    }

    [Fact]
    public void Constructor_Throws_WhenChangesIsEmpty()
    {
        Assert.Throws<ArgumentException>(
            () => new MemberUpdateRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), OccurredAt, []));
    }

    [Fact]
    public void Constructor_Throws_WhenChangesIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new MemberUpdateRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), OccurredAt, null!));
    }

    [Fact]
    public void Approve_TransitionsToApproved_AndRecordsReviewer()
    {
        var request = CreateRequest();
        var actorUserId = Guid.NewGuid();

        request.Approve(actorUserId, OccurredAt, "Looks correct");

        Assert.Equal(MemberUpdateRequestStatus.Approved, request.Status);
        Assert.Equal(actorUserId, request.ReviewedByUserId);
        Assert.Equal(OccurredAt, request.ReviewedAtUtc);
        Assert.Equal("Looks correct", request.ReviewRemarks);
    }

    [Fact]
    public void Approve_Throws_WhenAlreadyDecided()
    {
        var request = CreateRequest();
        request.Approve(Guid.NewGuid(), OccurredAt);

        Assert.Throws<ConflictException>(() => request.Approve(Guid.NewGuid(), OccurredAt));
    }

    [Fact]
    public void Approve_Throws_WhenActorUserIdIsEmpty()
    {
        var request = CreateRequest();

        Assert.Throws<ArgumentException>(() => request.Approve(Guid.Empty, OccurredAt));
    }

    [Fact]
    public void Reject_TransitionsToRejected_AndRecordsRemarks()
    {
        var request = CreateRequest();
        var actorUserId = Guid.NewGuid();

        request.Reject(actorUserId, OccurredAt, "Name does not match submitted ID");

        Assert.Equal(MemberUpdateRequestStatus.Rejected, request.Status);
        Assert.Equal(actorUserId, request.ReviewedByUserId);
        Assert.Equal("Name does not match submitted ID", request.ReviewRemarks);
    }

    [Fact]
    public void Reject_Throws_WhenAlreadyDecided()
    {
        var request = CreateRequest();
        request.Reject(Guid.NewGuid(), OccurredAt, "Not valid");

        Assert.Throws<ConflictException>(() => request.Reject(Guid.NewGuid(), OccurredAt, "Still not valid"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Reject_Throws_WhenRemarksAreMissing(string? remarks)
    {
        var request = CreateRequest();

        Assert.ThrowsAny<ArgumentException>(() => request.Reject(Guid.NewGuid(), OccurredAt, remarks!));
    }

    private static MemberUpdateRequest CreateRequest(IReadOnlyCollection<MemberUpdateRequestChangeInput>? changes = null)
    {
        return new MemberUpdateRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            OccurredAt,
            changes ?? [new MemberUpdateRequestChangeInput("LastName", "Dela Cruz", "Santos")]);
    }
}
