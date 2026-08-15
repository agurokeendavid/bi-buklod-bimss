using Bimss.Domain.Membership.ReferenceData;
using Bimss.Infrastructure.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.UnitTests.Membership;

public class ReferenceDataQueryServiceTests
{
    [Fact]
    public async Task ListCivilStatusesAsync_ReturnsOnlyActiveItems_OrderedByName()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using (var writeContext = CreateDbContext(databaseName))
        {
            var single = new CivilStatus(Guid.NewGuid(), "SINGLE", "Single");
            var married = new CivilStatus(Guid.NewGuid(), "MARRIED", "Married");
            var widowed = new CivilStatus(Guid.NewGuid(), "WIDOWED", "Widowed");
            widowed.SetActive(false);

            writeContext.CivilStatuses.AddRange(single, married, widowed);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreateDbContext(databaseName);
        var service = new ReferenceDataQueryService(readContext);

        var result = await service.ListCivilStatusesAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Married", result[0].Name);
        Assert.Equal("Single", result[1].Name);
        Assert.DoesNotContain(result, item => item.Code == "WIDOWED");
    }

    [Fact]
    public async Task ListSuffixesAsync_ReturnsActiveItems()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using (var writeContext = CreateDbContext(databaseName))
        {
            writeContext.Suffixes.Add(new Suffix(Guid.NewGuid(), "JR", "Jr."));
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreateDbContext(databaseName);
        var service = new ReferenceDataQueryService(readContext);

        var result = await service.ListSuffixesAsync(CancellationToken.None);

        var item = Assert.Single(result);
        Assert.Equal("JR", item.Code);
        Assert.Equal("Jr.", item.Name);
    }

    [Fact]
    public async Task ListOfficeUnitsAsync_ReturnsActiveItems()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using (var writeContext = CreateDbContext(databaseName))
        {
            writeContext.OfficeUnits.Add(new OfficeUnit(Guid.NewGuid(), "HEAD-OFFICE", "Head Office"));
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreateDbContext(databaseName);
        var service = new ReferenceDataQueryService(readContext);

        var result = await service.ListOfficeUnitsAsync(CancellationToken.None);

        var item = Assert.Single(result);
        Assert.Equal("HEAD-OFFICE", item.Code);
        Assert.Equal("Head Office", item.Name);
    }

    [Fact]
    public async Task ListMemberStatusReasonsAsync_ReturnsActiveItems()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using (var writeContext = CreateDbContext(databaseName))
        {
            writeContext.MemberStatusReasons.Add(new MemberStatusReason(Guid.NewGuid(), "RESIGNED", "Resigned from BI"));
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreateDbContext(databaseName);
        var service = new ReferenceDataQueryService(readContext);

        var result = await service.ListMemberStatusReasonsAsync(CancellationToken.None);

        var item = Assert.Single(result);
        Assert.Equal("RESIGNED", item.Code);
        Assert.Equal("Resigned from BI", item.Name);
    }

    private static BimssDbContext CreateDbContext(string databaseName)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>().UseInMemoryDatabase(databaseName);
        return new BimssDbContext(optionsBuilder.Options);
    }
}
