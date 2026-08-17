using Bimss.Domain.Membership;
using Bimss.Infrastructure.Membership;
using Bimss.IntegrationTests.Support;

namespace Bimss.IntegrationTests.Membership;

public class MemberUpdateRequestQueryServiceTests
{
    private static readonly DateTimeOffset OccurredAt = new(2026, 8, 17, 0, 0, 0, TimeSpan.Zero);

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task ListAsync_ReturnsRequests_WithMemberNames()
    {
        var memberId = Guid.NewGuid();
        Guid requestId;

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.Members.Add(new Member(
                memberId, "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
                Guid.NewGuid(), joiningReason: null, OccurredAt));

            var request = new MemberUpdateRequest(
                Guid.NewGuid(), memberId, Guid.NewGuid(), OccurredAt,
                [new MemberUpdateRequestChangeInput("FirstName", "Juan", "Juanito")]);
            requestId = request.Id;
            writeContext.MemberUpdateRequests.Add(request);

            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var queryService = new MemberUpdateRequestQueryService(readContext);

        var requests = await queryService.ListAsync(status: null, CancellationToken.None);

        var summary = Assert.Single(requests);
        Assert.Equal(requestId, summary.Id);
        Assert.Equal("Dela Cruz", summary.MemberLastName);
        Assert.Equal("Juan", summary.MemberFirstName);
        Assert.Equal(MemberUpdateRequestStatus.Pending, summary.Status);
    }

    [Fact]
    public async Task ListAsync_FiltersByStatus()
    {
        var memberId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.Members.Add(new Member(
                memberId, "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
                Guid.NewGuid(), joiningReason: null, OccurredAt));

            var pending = new MemberUpdateRequest(
                Guid.NewGuid(), memberId, Guid.NewGuid(), OccurredAt,
                [new MemberUpdateRequestChangeInput("FirstName", "Juan", "Juanito")]);

            var approved = new MemberUpdateRequest(
                Guid.NewGuid(), memberId, Guid.NewGuid(), OccurredAt,
                [new MemberUpdateRequestChangeInput("PlaceOfBirth", "Manila", "Cebu")]);
            approved.Approve(Guid.NewGuid(), OccurredAt);

            writeContext.MemberUpdateRequests.AddRange(pending, approved);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var queryService = new MemberUpdateRequestQueryService(readContext);

        var pendingOnly = await queryService.ListAsync(MemberUpdateRequestStatus.Pending, CancellationToken.None);

        var summary = Assert.Single(pendingOnly);
        Assert.Equal(MemberUpdateRequestStatus.Pending, summary.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDetail_WithChanges()
    {
        var memberId = Guid.NewGuid();
        Guid requestId;

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.Members.Add(new Member(
                memberId, "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
                Guid.NewGuid(), joiningReason: null, OccurredAt));

            var request = new MemberUpdateRequest(
                Guid.NewGuid(), memberId, Guid.NewGuid(), OccurredAt,
                [
                    new MemberUpdateRequestChangeInput("FirstName", "Juan", "Juanito"),
                    new MemberUpdateRequestChangeInput("PlaceOfBirth", "Manila", "Cebu"),
                ]);
            requestId = request.Id;
            writeContext.MemberUpdateRequests.Add(request);

            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var queryService = new MemberUpdateRequestQueryService(readContext);

        var detail = await queryService.GetByIdAsync(requestId, CancellationToken.None);

        Assert.NotNull(detail);
        Assert.Equal(requestId, detail!.Id);
        Assert.Equal("Dela Cruz", detail.MemberLastName);
        Assert.Equal(2, detail.Changes.Count);
        Assert.Contains(detail.Changes, c => c.FieldName == "FirstName" && c.NewValue == "Juanito");
    }

    [Fact]
    public async Task ListByMemberIdAsync_ReturnsOnlyThatMembersRequests()
    {
        var memberId = Guid.NewGuid();
        var otherMemberId = Guid.NewGuid();
        Guid requestId;

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.Members.Add(new Member(
                memberId, "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
                Guid.NewGuid(), joiningReason: null, OccurredAt));
            writeContext.Members.Add(new Member(
                otherMemberId, "Reyes", "Maria", middleName: null, suffixId: null, new DateOnly(1992, 3, 4), "Cebu",
                Guid.NewGuid(), joiningReason: null, OccurredAt));

            var request = new MemberUpdateRequest(
                Guid.NewGuid(), memberId, Guid.NewGuid(), OccurredAt,
                [new MemberUpdateRequestChangeInput("FirstName", "Juan", "Juanito")]);
            requestId = request.Id;

            var otherRequest = new MemberUpdateRequest(
                Guid.NewGuid(), otherMemberId, Guid.NewGuid(), OccurredAt,
                [new MemberUpdateRequestChangeInput("PlaceOfBirth", "Cebu", "Manila")]);

            writeContext.MemberUpdateRequests.AddRange(request, otherRequest);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var queryService = new MemberUpdateRequestQueryService(readContext);

        var requests = await queryService.ListByMemberIdAsync(memberId, CancellationToken.None);

        var summary = Assert.Single(requests);
        Assert.Equal(requestId, summary.Id);
        Assert.Equal(memberId, summary.MemberId);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenRequestDoesNotExist()
    {
        await using var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var queryService = new MemberUpdateRequestQueryService(dbContext);

        var detail = await queryService.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(detail);
    }
}
