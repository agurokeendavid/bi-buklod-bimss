using Bimss.Domain.Membership;
using Bimss.IntegrationTests.Support;
using Bimss.Infrastructure.Membership;

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
}
