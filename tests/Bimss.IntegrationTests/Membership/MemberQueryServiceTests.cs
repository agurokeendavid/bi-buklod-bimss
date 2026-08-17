using Bimss.Domain.Membership;
using Bimss.Domain.Membership.ReferenceData;
using Bimss.IntegrationTests.Support;
using Bimss.Infrastructure.Identity;
using Bimss.Infrastructure.Membership;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Membership;

public class MemberQueryServiceTests
{
    private static readonly DateTimeOffset OccurredAt = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetByIdAsync_ReturnsDetail_WithEmployment_WhenMemberExists()
    {
        var memberId = Guid.NewGuid();
        var civilStatusId = Guid.NewGuid();
        var officeUnitId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.Members.Add(new Member(
                memberId, "Dela Cruz", "Juan", "Santos", suffixId: null, new DateOnly(1990, 1, 1), "Manila",
                civilStatusId, "Referred", OccurredAt));
            writeContext.MemberEmployments.Add(new MemberEmployment(
                Guid.NewGuid(), memberId, "BI-00123", "Immigration Officer I", officeUnitId, new DateOnly(2020, 6, 1)));
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var queryService = new MemberQueryService(readContext);

        var detail = await queryService.GetByIdAsync(memberId, CancellationToken.None);

        Assert.NotNull(detail);
        Assert.Equal(memberId, detail!.Id);
        Assert.Equal("Dela Cruz", detail.LastName);
        Assert.Equal(civilStatusId, detail.CivilStatusId);
        Assert.Equal(MemberStatus.PendingVerification, detail.Status);
        Assert.Equal("BI-00123", detail.EmployeeNumber);
        Assert.Equal("Immigration Officer I", detail.PositionDesignation);
        Assert.Equal(officeUnitId, detail.OfficeUnitId);
        Assert.Equal(new DateOnly(2020, 6, 1), detail.PermanentAppointmentDate);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDetail_WithNullEmploymentFields_WhenNoEmploymentExists()
    {
        var memberId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.Members.Add(new Member(
                memberId, "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
                Guid.NewGuid(), joiningReason: null, OccurredAt));
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var queryService = new MemberQueryService(readContext);

        var detail = await queryService.GetByIdAsync(memberId, CancellationToken.None);

        Assert.NotNull(detail);
        Assert.Null(detail!.EmployeeNumber);
        Assert.Null(detail.PositionDesignation);
        Assert.Null(detail.OfficeUnitId);
        Assert.Null(detail.PermanentAppointmentDate);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenMemberDoesNotExist()
    {
        await using var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var queryService = new MemberQueryService(dbContext);

        var detail = await queryService.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(detail);
    }

    [Fact]
    public async Task ListAsync_ReturnsAllMembers_AsSummaries()
    {
        var firstMemberId = Guid.NewGuid();
        var secondMemberId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.Members.Add(new Member(
                firstMemberId, "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
                Guid.NewGuid(), joiningReason: null, OccurredAt));
            writeContext.MemberEmployments.Add(new MemberEmployment(
                Guid.NewGuid(), firstMemberId, "BI-00123", "Immigration Officer I", Guid.NewGuid(), null));

            writeContext.Members.Add(new Member(
                secondMemberId, "Santos", "Maria", middleName: null, suffixId: null, new DateOnly(1992, 2, 2), "Cebu",
                Guid.NewGuid(), joiningReason: null, OccurredAt));
            writeContext.MemberEmployments.Add(new MemberEmployment(
                Guid.NewGuid(), secondMemberId, "BI-00456", "Immigration Officer II", Guid.NewGuid(), null));

            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var queryService = new MemberQueryService(readContext);

        var summaries = await queryService.ListAsync(CancellationToken.None);

        Assert.Equal(2, summaries.Count);
        Assert.Contains(summaries, s => s.Id == firstMemberId && s.EmployeeNumber == "BI-00123");
        Assert.Contains(summaries, s => s.Id == secondMemberId && s.EmployeeNumber == "BI-00456");
    }

    [Fact]
    public async Task ListAsync_ReturnsEmptyList_WhenNoMembersExist()
    {
        await using var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var queryService = new MemberQueryService(dbContext);

        var summaries = await queryService.ListAsync(CancellationToken.None);

        Assert.Empty(summaries);
    }

    [Fact]
    public async Task ListStatusHistoryAsync_ReturnsTransitionsInOrder_ForThatMemberOnly()
    {
        var memberId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var member = new Member(
                memberId, "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
                Guid.NewGuid(), joiningReason: null, OccurredAt);
            member.Verify(actorUserId, OccurredAt.AddDays(1), "Documents checked");
            writeContext.Members.Add(member);

            var otherMember = new Member(
                Guid.NewGuid(), "Santos", "Maria", middleName: null, suffixId: null, new DateOnly(1992, 2, 2), "Cebu",
                Guid.NewGuid(), joiningReason: null, OccurredAt);
            writeContext.Members.Add(otherMember);

            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var queryService = new MemberQueryService(readContext);

        var history = await queryService.ListStatusHistoryAsync(memberId, CancellationToken.None);

        Assert.Equal(2, history.Count);
        Assert.Null(history[0].FromStatus);
        Assert.Equal(MemberStatus.PendingVerification, history[0].ToStatus);
        Assert.Equal(MemberStatus.PendingVerification, history[1].FromStatus);
        Assert.Equal(MemberStatus.Active, history[1].ToStatus);
        Assert.Equal(actorUserId, history[1].ActorUserId);
        Assert.Equal("Documents checked", history[1].Remarks);
    }

    [Fact]
    public async Task GetMyProfileByUserIdAsync_ReturnsProfile_WithResolvedReferenceNames()
    {
        var userId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var suffixId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var civilStatus = new CivilStatus(Guid.NewGuid(), "SGL", "Single");
            var officeUnit = new OfficeUnit(Guid.NewGuid(), "POD", "Port Operations Division");
            var suffix = new Suffix(suffixId, "JR", "Jr.");
            writeContext.CivilStatuses.Add(civilStatus);
            writeContext.OfficeUnits.Add(officeUnit);
            writeContext.Suffixes.Add(suffix);

            writeContext.Members.Add(new Member(
                memberId, "Dela Cruz", "Juan", middleName: null, suffixId, new DateOnly(1990, 1, 1), "Manila",
                civilStatus.Id, "Referred", OccurredAt));
            writeContext.MemberEmployments.Add(new MemberEmployment(
                Guid.NewGuid(), memberId, "BI-00123", "Immigration Officer I", officeUnit.Id, new DateOnly(2020, 6, 1)));

            writeContext.Users.Add(new ApplicationUser { Id = Guid.NewGuid(), UserName = "member.dev", MemberId = memberId });

            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var queryService = new MemberQueryService(readContext);

        var profile = await queryService.GetMyProfileByUserIdAsync(userId: default, CancellationToken.None);
        Assert.Null(profile);

        var linkedUserId = await readContext.Users.Where(u => u.UserName == "member.dev").Select(u => u.Id).SingleAsync();
        var linkedProfile = await queryService.GetMyProfileByUserIdAsync(linkedUserId, CancellationToken.None);

        Assert.NotNull(linkedProfile);
        Assert.Equal(memberId, linkedProfile!.Id);
        Assert.Equal("Dela Cruz", linkedProfile.LastName);
        Assert.Equal(suffixId, linkedProfile.SuffixId);
        Assert.Equal("Jr.", linkedProfile.SuffixName);
        Assert.Equal("Single", linkedProfile.CivilStatusName);
        Assert.Equal("Port Operations Division", linkedProfile.OfficeUnitName);
        Assert.Equal("BI-00123", linkedProfile.EmployeeNumber);
    }

    [Fact]
    public async Task GetMyProfileByUserIdAsync_ReturnsNull_WhenUserHasNoLinkedMember()
    {
        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.Users.Add(new ApplicationUser { Id = Guid.NewGuid(), UserName = "officer.dev", MemberId = null });
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var queryService = new MemberQueryService(readContext);
        var userId = await readContext.Users.Where(u => u.UserName == "officer.dev").Select(u => u.Id).SingleAsync();

        var profile = await queryService.GetMyProfileByUserIdAsync(userId, CancellationToken.None);

        Assert.Null(profile);
    }

    [Fact]
    public async Task GetMyProfileByUserIdAsync_ReturnsProfileWithNullSuffixName_WhenMemberHasNoSuffix()
    {
        var memberId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var civilStatus = new CivilStatus(Guid.NewGuid(), "SGL", "Single");
            var officeUnit = new OfficeUnit(Guid.NewGuid(), "POD", "Port Operations Division");
            writeContext.CivilStatuses.Add(civilStatus);
            writeContext.OfficeUnits.Add(officeUnit);

            writeContext.Members.Add(new Member(
                memberId, "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
                civilStatus.Id, joiningReason: null, OccurredAt));
            writeContext.MemberEmployments.Add(new MemberEmployment(
                Guid.NewGuid(), memberId, "BI-00123", "Immigration Officer I", officeUnit.Id, null));
            writeContext.Users.Add(new ApplicationUser { Id = Guid.NewGuid(), UserName = "member2.dev", MemberId = memberId });

            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var queryService = new MemberQueryService(readContext);
        var userId = await readContext.Users.Where(u => u.UserName == "member2.dev").Select(u => u.Id).SingleAsync();

        var profile = await queryService.GetMyProfileByUserIdAsync(userId, CancellationToken.None);

        Assert.NotNull(profile);
        Assert.Null(profile!.SuffixName);
    }

    [Fact]
    public async Task GetMemberIdByUserIdAsync_ReturnsTheLinkedMemberId()
    {
        var memberId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.Users.Add(new ApplicationUser { Id = userId, UserName = "member3.dev", MemberId = memberId });
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var queryService = new MemberQueryService(readContext);

        var result = await queryService.GetMemberIdByUserIdAsync(userId, CancellationToken.None);

        Assert.Equal(memberId, result);
    }

    [Fact]
    public async Task GetMemberIdByUserIdAsync_ReturnsNull_WhenUserHasNoLinkedMember()
    {
        var userId = Guid.NewGuid();

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            writeContext.Users.Add(new ApplicationUser { Id = userId, UserName = "officer2.dev", MemberId = null });
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var queryService = new MemberQueryService(readContext);

        var result = await queryService.GetMemberIdByUserIdAsync(userId, CancellationToken.None);

        Assert.Null(result);
    }
}
