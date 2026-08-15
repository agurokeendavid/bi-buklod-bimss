using Bimss.Domain.Membership;
using Bimss.IntegrationTests.Support;
using Bimss.Infrastructure.Membership;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Membership;

public class MemberRepositoryTests
{
    private static readonly DateTimeOffset OccurredAt = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task EmployeeNumberExistsAsync_ReturnsFalse_WhenNoMemberHasThatNumber()
    {
        await using var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var repository = new MemberRepository(dbContext);

        var exists = await repository.EmployeeNumberExistsAsync("BI-00123", CancellationToken.None);

        Assert.False(exists);
    }

    [Fact]
    public async Task AddAsync_PersistsMemberAndEmployment_Atomically()
    {
        var memberId = Guid.NewGuid();
        var member = new Member(
            memberId, "Dela Cruz", "Juan", "Santos", suffixId: null, new DateOnly(1990, 1, 1), "Manila", Guid.NewGuid(), "Referred", OccurredAt);
        var employment = new MemberEmployment(
            Guid.NewGuid(), memberId, "BI-00123", "Immigration Officer I", Guid.NewGuid(), new DateOnly(2020, 6, 1));

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var repository = new MemberRepository(writeContext);
            await repository.AddAsync(member, employment, CancellationToken.None);
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persistedMember = await readContext.Members.SingleAsync();
        var persistedEmployment = await readContext.MemberEmployments.SingleAsync();

        Assert.Equal(memberId, persistedMember.Id);
        Assert.Equal(memberId, persistedEmployment.MemberId);
        Assert.Equal("BI-00123", persistedEmployment.EmployeeNumber);
    }

    [Fact]
    public async Task EmployeeNumberExistsAsync_ReturnsTrue_AfterAdding()
    {
        var memberId = Guid.NewGuid();
        var member = new Member(
            memberId, "Dela Cruz", "Juan", "Santos", suffixId: null, new DateOnly(1990, 1, 1), "Manila", Guid.NewGuid(), "Referred", OccurredAt);
        var employment = new MemberEmployment(
            Guid.NewGuid(), memberId, "BI-00123", "Immigration Officer I", Guid.NewGuid(), new DateOnly(2020, 6, 1));

        await using (var writeContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var repository = new MemberRepository(writeContext);
            await repository.AddAsync(member, employment, CancellationToken.None);
        }

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var readRepository = new MemberRepository(readContext);

        Assert.True(await readRepository.EmployeeNumberExistsAsync("BI-00123", CancellationToken.None));
        Assert.False(await readRepository.EmployeeNumberExistsAsync("BI-99999", CancellationToken.None));
    }
}
