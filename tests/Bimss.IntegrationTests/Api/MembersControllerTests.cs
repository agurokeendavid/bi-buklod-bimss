using System.Net;
using System.Net.Http.Json;
using Bimss.Contracts.Membership;
using Bimss.Domain.Authorization;
using Bimss.Domain.Membership;
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

public class MembersControllerTests : IDisposable
{
    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly WebApplicationFactory<Program> _factory;

    public MembersControllerTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<BimssDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<BimssDbContext>>();
                services.AddDbContext<BimssDbContext>(options => options.UseInMemoryDatabase(_databaseName));

                // The claims transformation queries a real database on every
                // authenticated request; these tests exercise policy
                // enforcement given specific claims via TestAuthHandler, so
                // it's removed rather than exercised here (matches
                // DiagnosticsApiFactory's precedent).
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
    public async Task List_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/members");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_ReturnsForbidden_WithoutThePermission()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");

        var response = await client.GetAsync("/api/members");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task List_ReturnsMembers_WithThePermission()
    {
        await SeedMemberAsync("Dela Cruz", "Juan", "BI-00123");

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var response = await client.GetAsync("/api/members");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var members = await response.Content.ReadFromJsonAsync<List<MemberSummaryResponse>>();
        Assert.NotNull(members);
        var member = Assert.Single(members!);
        Assert.Equal("Dela Cruz", member.LastName);
        Assert.Equal("Juan", member.FirstName);
        Assert.Equal("BI-00123", member.EmployeeNumber);
        Assert.Equal("PendingVerification", member.Status);
    }

    [Fact]
    public async Task List_ReturnsEmptyArray_WhenNoMembersExist()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var response = await client.GetAsync("/api/members");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var members = await response.Content.ReadFromJsonAsync<List<MemberSummaryResponse>>();
        Assert.NotNull(members);
        Assert.Empty(members!);
    }

    private async Task SeedMemberAsync(string lastName, string firstName, string employeeNumber)
    {
        await using var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName);

        var member = new Member(
            Guid.NewGuid(), lastName, firstName, middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
            Guid.NewGuid(), joiningReason: null, DateTimeOffset.UtcNow);
        dbContext.Members.Add(member);
        dbContext.MemberEmployments.Add(
            new MemberEmployment(Guid.NewGuid(), member.Id, employeeNumber, "Immigration Officer I", Guid.NewGuid(), null));

        await dbContext.SaveChangesAsync();
    }
}
