using System.Net;
using System.Net.Http.Json;
using Bimss.Contracts.Membership;
using Bimss.Domain.Authorization;
using Bimss.Domain.Membership;
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

public class MyContactControllerTests : IDisposable
{
    private static readonly DateTimeOffset OccurredAt = new(2026, 8, 17, 0, 0, 0, TimeSpan.Zero);

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly WebApplicationFactory<Program> _factory;

    public MyContactControllerTests()
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

        var response = await client.GetAsync("/api/my/contact");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsForbidden_WithoutViewSelfPermission()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");

        var response = await client.GetAsync("/api/my/contact");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenTheCallerHasNoLinkedMember()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.ViewSelf);

        var response = await client.GetAsync("/api/my/contact");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsCurrentContact_ForALinkedMember()
    {
        var userId = Guid.NewGuid();

        await using (var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            var member = await SeedMemberAsync(dbContext, userId);
            dbContext.MemberContacts.Add(new MemberContact(Guid.NewGuid(), member.Id, "8888-1234", "0917 000 0000", "member@example.com"));
            dbContext.MemberAddresses.Add(new MemberAddress(Guid.NewGuid(), member.Id, MemberAddressType.Present, "123 Present St."));
            await dbContext.SaveChangesAsync();
        }

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.ViewSelf);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());

        var response = await client.GetAsync("/api/my/contact");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var contact = await response.Content.ReadFromJsonAsync<MyContactResponse>();
        Assert.NotNull(contact);
        Assert.Equal("0917 000 0000", contact!.MobileNumber);
        Assert.Equal("member@example.com", contact.Email);
        Assert.Equal("123 Present St.", contact.PresentAddress);
        Assert.Null(contact.PermanentAddress);
    }

    [Fact]
    public async Task Update_ReturnsForbidden_WithoutManageSelfPermission()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");

        var response = await client.PutAsJsonAsync("/api/my/contact", BuildRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenMobileNumberIsMissing()
    {
        var userId = Guid.NewGuid();

        await using (var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            await SeedMemberAsync(dbContext, userId);
        }

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.ManageSelf);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());

        var request = BuildRequest();
        request.MobileNumber = string.Empty;
        var response = await client.PutAsJsonAsync("/api/my/contact", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_Succeeds_AndPersistsContactAndAddresses()
    {
        var userId = Guid.NewGuid();
        Member member = null!;

        await using (var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            member = await SeedMemberAsync(dbContext, userId);
        }

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.ManageSelf);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());

        var response = await client.PutAsJsonAsync("/api/my/contact", BuildRequest());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<MyContactResponse>();
        Assert.NotNull(body);
        Assert.Equal("0917 000 0000", body!.MobileNumber);
        Assert.Equal("123 Present St.", body.PresentAddress);

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persistedContact = await readContext.MemberContacts.SingleAsync(c => c.MemberId == member.Id);
        Assert.Equal("member@example.com", persistedContact.Email);
    }

    private static async Task<Member> SeedMemberAsync(BimssDbContext dbContext, Guid userId)
    {
        var member = new Member(
            Guid.NewGuid(), "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
            Guid.NewGuid(), joiningReason: null, OccurredAt);

        dbContext.Members.Add(member);
        dbContext.Users.Add(new ApplicationUser { Id = userId, UserName = "member.dev", MemberId = member.Id });

        await dbContext.SaveChangesAsync();

        return member;
    }

    private static UpdateMyContactRequest BuildRequest()
    {
        return new UpdateMyContactRequest
        {
            Landline = "8888-1234",
            MobileNumber = "0917 000 0000",
            Email = "member@example.com",
            PresentAddress = "123 Present St.",
            PermanentAddress = null,
        };
    }
}
