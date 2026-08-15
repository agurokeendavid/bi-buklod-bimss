using System.Net;
using System.Net.Http.Json;
using Bimss.Contracts.Membership;
using Bimss.Domain.Authorization;
using Bimss.Domain.Membership.ReferenceData;
using Bimss.Infrastructure.Persistence;
using Bimss.IntegrationTests.Support;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bimss.IntegrationTests.Api;

public class ReferenceDataControllerTests : IDisposable
{
    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly WebApplicationFactory<Program> _factory;

    public ReferenceDataControllerTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<BimssDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<BimssDbContext>>();
                services.AddDbContext<BimssDbContext>(options => options.UseInMemoryDatabase(_databaseName));

                services.RemoveAll<IClaimsTransformation>();

                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

                services.PostConfigure<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                });
            });
        });
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task ListCivilStatuses_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/reference-data/civil-statuses");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ListCivilStatuses_ReturnsForbidden_WithoutThePermission()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");

        var response = await client.GetAsync("/api/reference-data/civil-statuses");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ListCivilStatuses_ReturnsActiveItems_WithThePermission()
    {
        await using (var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            dbContext.CivilStatuses.Add(new CivilStatus(Guid.NewGuid(), "SINGLE", "Single"));
            await dbContext.SaveChangesAsync();
        }

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var response = await client.GetAsync("/api/reference-data/civil-statuses");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<ReferenceDataItemResponse>>();
        Assert.NotNull(items);
        var item = Assert.Single(items!);
        Assert.Equal("SINGLE", item.Code);
        Assert.Equal("Single", item.Name);
    }

    [Fact]
    public async Task ListSuffixes_ReturnsActiveItems_WithThePermission()
    {
        await using (var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            dbContext.Suffixes.Add(new Suffix(Guid.NewGuid(), "JR", "Jr."));
            await dbContext.SaveChangesAsync();
        }

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var response = await client.GetAsync("/api/reference-data/suffixes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<ReferenceDataItemResponse>>();
        Assert.NotNull(items);
        Assert.Single(items!);
    }

    [Fact]
    public async Task ListOfficeUnits_ReturnsActiveItems_WithThePermission()
    {
        await using (var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            dbContext.OfficeUnits.Add(new OfficeUnit(Guid.NewGuid(), "HEAD-OFFICE", "Head Office"));
            await dbContext.SaveChangesAsync();
        }

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var response = await client.GetAsync("/api/reference-data/office-units");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<ReferenceDataItemResponse>>();
        Assert.NotNull(items);
        Assert.Single(items!);
    }

    [Fact]
    public async Task ListMemberStatusReasons_ReturnsActiveItems_WithThePermission()
    {
        await using (var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            dbContext.MemberStatusReasons.Add(new MemberStatusReason(Guid.NewGuid(), "RESIGNED", "Resigned from BI"));
            await dbContext.SaveChangesAsync();
        }

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var response = await client.GetAsync("/api/reference-data/member-status-reasons");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<ReferenceDataItemResponse>>();
        Assert.NotNull(items);
        Assert.Single(items!);
    }
}
