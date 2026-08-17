using System.Net;
using System.Net.Http.Json;
using Bimss.Contracts.Membership;
using Bimss.Domain.Authorization;
using Bimss.Domain.Membership;
using Bimss.Domain.Membership.ReferenceData;
using Bimss.Infrastructure.Identity;
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

public class MyProfileControllerTests : IDisposable
{
    private static readonly DateTimeOffset OccurredAt = new(2026, 8, 17, 0, 0, 0, TimeSpan.Zero);

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly WebApplicationFactory<Program> _factory;

    public MyProfileControllerTests()
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

    public void Dispose()
    {
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Get_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/my/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsForbidden_WithoutViewSelfPermission()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");

        var response = await client.GetAsync("/api/my/profile");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenTheCallerHasNoLinkedMember()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.ViewSelf);

        var response = await client.GetAsync("/api/my/profile");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsResolvedProfile_ForALinkedMember()
    {
        var userId = Guid.NewGuid();

        await using (var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var civilStatus = new CivilStatus(Guid.NewGuid(), "SGL", "Single");
            var officeUnit = new OfficeUnit(Guid.NewGuid(), "POD", "Port Operations Division");
            dbContext.CivilStatuses.Add(civilStatus);
            dbContext.OfficeUnits.Add(officeUnit);

            var member = new Member(
                Guid.NewGuid(), "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
                civilStatus.Id, "Referred", OccurredAt);
            dbContext.Members.Add(member);
            dbContext.MemberEmployments.Add(
                new MemberEmployment(Guid.NewGuid(), member.Id, "BI-00123", "Immigration Officer I", officeUnit.Id, null));
            dbContext.Users.Add(new ApplicationUser { Id = userId, UserName = "member.dev", MemberId = member.Id });

            await dbContext.SaveChangesAsync();
        }

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.ViewSelf);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());

        var response = await client.GetAsync("/api/my/profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<MyProfileResponse>();
        Assert.NotNull(profile);
        Assert.Equal("Dela Cruz", profile!.LastName);
        Assert.Equal("Single", profile.CivilStatusName);
        Assert.Equal("Port Operations Division", profile.OfficeUnitName);
        Assert.Equal("BI-00123", profile.EmployeeNumber);
    }
}
