using System.Security.Claims;
using Bimss.Domain.Authorization;
using Bimss.Infrastructure.Identity;
using Bimss.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Bimss.IntegrationTests.Identity;

public class PermissionClaimsTransformationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _sqlContainer.DisposeAsync().AsTask();

    [Fact]
    public async Task TransformAsync_AddsPermissionClaims_ForUsersRoleAssignments()
    {
        await using var dbContext = CreateDbContext();

        var role = new ApplicationRole { Id = Guid.NewGuid(), Name = "Finance", NormalizedName = "FINANCE" };
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "finance.officer",
            NormalizedUserName = "FINANCE.OFFICER",
        };

        dbContext.Roles.Add(role);
        dbContext.Users.Add(user);
        dbContext.UserRoles.Add(new IdentityUserRole<Guid> { UserId = user.Id, RoleId = role.Id });
        dbContext.RolePermissions.AddRange(
            new RolePermission { RoleId = role.Id, PermissionName = Permission.Contribution.Manage },
            new RolePermission { RoleId = role.Id, PermissionName = Permission.Report.ViewFinance });
        await dbContext.SaveChangesAsync();

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())], "TestScheme");
        var principal = new ClaimsPrincipal(identity);

        var transformation = new PermissionClaimsTransformation(dbContext);
        var transformed = await transformation.TransformAsync(principal);

        var permissionClaims = transformed.FindAll(Permission.ClaimType).Select(claim => claim.Value).ToList();
        Assert.Equal(
            new[] { Permission.Contribution.Manage, Permission.Report.ViewFinance }.OrderBy(name => name),
            permissionClaims.OrderBy(name => name));
    }

    [Fact]
    public async Task TransformAsync_AddsNoPermissionClaims_ForAUserWithNoRoleAssignments()
    {
        await using var dbContext = CreateDbContext();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "no.roles",
            NormalizedUserName = "NO.ROLES",
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())], "TestScheme");
        var principal = new ClaimsPrincipal(identity);

        var transformation = new PermissionClaimsTransformation(dbContext);
        var transformed = await transformation.TransformAsync(principal);

        Assert.Empty(transformed.FindAll(Permission.ClaimType));
    }

    [Fact]
    public async Task TransformAsync_ReturnsPrincipalUnchanged_WhenNotAuthenticated()
    {
        await using var dbContext = CreateDbContext();

        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var transformation = new PermissionClaimsTransformation(dbContext);
        var transformed = await transformation.TransformAsync(principal);

        Assert.Empty(transformed.FindAll(Permission.ClaimType));
    }

    private BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseSqlServer(_sqlContainer.GetConnectionString());

        return new BimssDbContext(optionsBuilder.Options);
    }
}
