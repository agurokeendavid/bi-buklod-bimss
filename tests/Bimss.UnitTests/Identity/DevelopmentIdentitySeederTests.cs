using Bimss.Domain.Authorization;
using Bimss.Infrastructure.Identity;
using Bimss.Infrastructure.Identity.Seeding;
using Bimss.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bimss.UnitTests.Identity;

public class DevelopmentIdentitySeederTests
{
    [Fact]
    public async Task SeedAsync_CreatesTheExpectedRoles_WithTheirPermissionAssignments()
    {
        await using var provider = BuildProvider();

        await DevelopmentIdentitySeeder.SeedAsync(provider);

        using var scope = provider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<BimssDbContext>();

        var adminRole = await roleManager.FindByNameAsync("Administrator");
        Assert.NotNull(adminRole);
        var adminPermissions = await dbContext.RolePermissions
            .Where(rolePermission => rolePermission.RoleId == adminRole.Id)
            .Select(rolePermission => rolePermission.PermissionName)
            .ToListAsync();
        Assert.Equal(Permission.All.OrderBy(name => name), adminPermissions.OrderBy(name => name));

        var financeRole = await roleManager.FindByNameAsync("FinanceOfficer");
        Assert.NotNull(financeRole);
        var financePermissions = await dbContext.RolePermissions
            .Where(rolePermission => rolePermission.RoleId == financeRole.Id)
            .Select(rolePermission => rolePermission.PermissionName)
            .ToListAsync();
        Assert.Equal(
            new[] { Permission.Contribution.Manage, Permission.Loan.Review, Permission.Loan.Approve, Permission.Loan.Release, Permission.Report.ViewFinance }
                .OrderBy(name => name),
            financePermissions.OrderBy(name => name));
    }

    [Fact]
    public async Task SeedAsync_CreatesTheExpectedDevUsers_AssignedToTheirRoles()
    {
        await using var provider = BuildProvider();

        await DevelopmentIdentitySeeder.SeedAsync(provider);

        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var financeOfficer = await userManager.FindByNameAsync("finance.officer.dev");
        Assert.NotNull(financeOfficer);
        Assert.True(await userManager.IsInRoleAsync(financeOfficer, "FinanceOfficer"));
        Assert.True(await userManager.CheckPasswordAsync(financeOfficer, "Dev-Only-Passw0rd!23"));
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent_WhenCalledMultipleTimes()
    {
        await using var provider = BuildProvider();

        await DevelopmentIdentitySeeder.SeedAsync(provider);
        await DevelopmentIdentitySeeder.SeedAsync(provider);

        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BimssDbContext>();

        Assert.Equal(6, await dbContext.Roles.CountAsync());
        Assert.Equal(6, await dbContext.Users.CountAsync());
        Assert.Equal(Permission.All.Count, await dbContext.RolePermissions.CountAsync(rp => rp.RoleId == dbContext.Roles.First(r => r.Name == "Administrator").Id));
    }

    private static ServiceProvider BuildProvider()
    {
        var databaseName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<BimssDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddBimssIdentity();

        return services.BuildServiceProvider();
    }
}
