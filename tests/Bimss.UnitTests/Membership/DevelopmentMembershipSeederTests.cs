using Bimss.Application;
using Bimss.Domain.Membership;
using Bimss.Infrastructure.Auditing;
using Bimss.Infrastructure.Membership;
using Bimss.Infrastructure.Membership.Seeding;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bimss.UnitTests.Membership;

public class DevelopmentMembershipSeederTests
{
    [Fact]
    public async Task SeedAsync_CreatesExpectedReferenceData()
    {
        await using var provider = BuildProvider();

        await DevelopmentMembershipSeeder.SeedAsync(provider);

        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BimssDbContext>();

        Assert.Equal(4, await dbContext.CivilStatuses.CountAsync());
        Assert.Equal(3, await dbContext.Suffixes.CountAsync());
        Assert.Equal(3, await dbContext.OfficeUnits.CountAsync());
        Assert.Equal(3, await dbContext.EducationalAttainments.CountAsync());
        Assert.Equal(2, await dbContext.EligibilityTypes.CountAsync());
        Assert.Equal(4, await dbContext.RelationshipTypes.CountAsync());
        Assert.Equal(3, await dbContext.MemberStatusReasons.CountAsync());

        Assert.Contains(await dbContext.CivilStatuses.ToListAsync(), c => c.Code == "SINGLE");
        Assert.Contains(await dbContext.OfficeUnits.ToListAsync(), o => o.Code == "HEAD-OFFICE");
    }

    [Fact]
    public async Task SeedAsync_CreatesExpectedMembers_WithCorrectStatuses()
    {
        await using var provider = BuildProvider();

        await DevelopmentMembershipSeeder.SeedAsync(provider);

        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BimssDbContext>();

        Assert.Equal(3, await dbContext.Members.CountAsync());
        Assert.Equal(3, await dbContext.MemberEmployments.CountAsync());

        var pending = await dbContext.MemberEmployments.SingleAsync(e => e.EmployeeNumber == "DEV-00001");
        var pendingMember = await dbContext.Members.SingleAsync(m => m.Id == pending.MemberId);
        Assert.Equal(MemberStatus.PendingVerification, pendingMember.Status);

        var active = await dbContext.MemberEmployments.SingleAsync(e => e.EmployeeNumber == "DEV-00002");
        var activeMember = await dbContext.Members.SingleAsync(m => m.Id == active.MemberId);
        Assert.Equal(MemberStatus.Active, activeMember.Status);

        var inactive = await dbContext.MemberEmployments.SingleAsync(e => e.EmployeeNumber == "DEV-00003");
        var inactiveMember = await dbContext.Members.SingleAsync(m => m.Id == inactive.MemberId);
        Assert.Equal(MemberStatus.Inactive, inactiveMember.Status);
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent_WhenCalledMultipleTimes()
    {
        await using var provider = BuildProvider();

        await DevelopmentMembershipSeeder.SeedAsync(provider);
        await DevelopmentMembershipSeeder.SeedAsync(provider);

        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BimssDbContext>();

        Assert.Equal(4, await dbContext.CivilStatuses.CountAsync());
        Assert.Equal(3, await dbContext.Members.CountAsync());
        Assert.Equal(3, await dbContext.MemberEmployments.CountAsync());
    }

    private static ServiceProvider BuildProvider()
    {
        var databaseName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<BimssDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddBimssMembership();
        services.AddBimssAuditing();
        services.AddBimssApplication();

        return services.BuildServiceProvider();
    }
}
