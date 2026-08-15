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

    [Fact]
    public async Task GetById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/members/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsForbidden_WithoutThePermission()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");

        var response = await client.GetAsync($"/api/members/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMemberDoesNotExist()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var response = await client.GetAsync($"/api/members/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsMemberDetail_WithThePermission()
    {
        var memberId = await SeedMemberAsync("Dela Cruz", "Juan", "BI-00123");

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var response = await client.GetAsync($"/api/members/{memberId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var member = await response.Content.ReadFromJsonAsync<MemberDetailResponse>();
        Assert.NotNull(member);
        Assert.Equal(memberId, member!.Id);
        Assert.Equal("Dela Cruz", member.LastName);
        Assert.Equal("Juan", member.FirstName);
        Assert.Equal("Manila", member.PlaceOfBirth);
        Assert.Equal("BI-00123", member.EmployeeNumber);
        Assert.Equal("Immigration Officer I", member.PositionDesignation);
        Assert.Equal("PendingVerification", member.Status);
    }

    [Fact]
    public async Task Create_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/members", CreateValidRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsForbidden_WithoutThePermission()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");

        var response = await client.PostAsJsonAsync("/api/members", CreateValidRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenLastNameIsMissing()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var request = CreateValidRequest();
        request.LastName = string.Empty;

        var response = await client.PostAsJsonAsync("/api/members", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_AndPersistsTheMember_WithThePermission()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var response = await client.PostAsJsonAsync("/api/members", CreateValidRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreateMemberResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.Id);

        var getResponse = await client.GetAsync($"/api/members/{body.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var member = await getResponse.Content.ReadFromJsonAsync<MemberDetailResponse>();
        Assert.NotNull(member);
        Assert.Equal("Dela Cruz", member!.LastName);
        Assert.Equal("BI-00999", member.EmployeeNumber);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenEmployeeNumberAlreadyRegistered()
    {
        await SeedMemberAsync("Existing", "Member", "BI-00999");

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var response = await client.PostAsJsonAsync("/api/members", CreateValidRequest());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        using var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/members/{Guid.NewGuid()}", CreateValidUpdateRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsForbidden_WithoutThePermission()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");

        var response = await client.PutAsJsonAsync($"/api/members/{Guid.NewGuid()}", CreateValidUpdateRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMemberDoesNotExist()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var response = await client.PutAsJsonAsync($"/api/members/{Guid.NewGuid()}", CreateValidUpdateRequest());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenLastNameIsMissing()
    {
        var memberId = await SeedMemberAsync("Dela Cruz", "Juan", "BI-00123");

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var request = CreateValidUpdateRequest();
        request.LastName = string.Empty;

        var response = await client.PutAsJsonAsync($"/api/members/{memberId}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOk_AndPersistsChanges_WithThePermission()
    {
        var memberId = await SeedMemberAsync("Dela Cruz", "Juan", "BI-00123");

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var request = CreateValidUpdateRequest();
        request.LastName = "Reyes";
        request.PositionDesignation = "Senior Immigration Officer";

        var response = await client.PutAsJsonAsync($"/api/members/{memberId}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<MemberDetailResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Reyes", updated!.LastName);
        Assert.Equal("Senior Immigration Officer", updated.PositionDesignation);
        // BI Employee Number is a business identifier and not editable via this endpoint.
        Assert.Equal("BI-00123", updated.EmployeeNumber);

        var getResponse = await client.GetAsync($"/api/members/{memberId}");
        var persisted = await getResponse.Content.ReadFromJsonAsync<MemberDetailResponse>();
        Assert.NotNull(persisted);
        Assert.Equal("Reyes", persisted!.LastName);
        Assert.Equal("Senior Immigration Officer", persisted.PositionDesignation);
    }

    [Fact]
    public async Task Verify_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/members/{Guid.NewGuid()}/verify", new VerifyMemberRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Verify_ReturnsForbidden_WithoutTheVerifyPermission()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        // Manage alone is not enough for Verify — it's gated on its own
        // dedicated Membership.Verify permission (Verify is not a superset
        // action of Manage; see MembersController's authorization comment).
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var response = await client.PostAsJsonAsync($"/api/members/{Guid.NewGuid()}/verify", new VerifyMemberRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Verify_ReturnsNotFound_WhenMemberDoesNotExist()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Verify);

        var response = await client.PostAsJsonAsync($"/api/members/{Guid.NewGuid()}/verify", new VerifyMemberRequest());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Verify_ReturnsOk_AndTransitionsToActive_WithThePermission()
    {
        var memberId = await SeedMemberAsync("Dela Cruz", "Juan", "BI-00123");

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Verify);

        var response = await client.PostAsJsonAsync(
            $"/api/members/{memberId}/verify", new VerifyMemberRequest { Remarks = "Documents checked" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var member = await response.Content.ReadFromJsonAsync<MemberDetailResponse>();
        Assert.NotNull(member);
        Assert.Equal("Active", member!.Status);
    }

    [Fact]
    public async Task Verify_ReturnsConflict_WhenNotPendingVerification()
    {
        var memberId = await SeedMemberAsync("Dela Cruz", "Juan", "BI-00123");

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Verify);

        await client.PostAsJsonAsync($"/api/members/{memberId}/verify", new VerifyMemberRequest());
        var response = await client.PostAsJsonAsync($"/api/members/{memberId}/verify", new VerifyMemberRequest());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/members/{Guid.NewGuid()}/deactivate", new DeactivateMemberRequest { ReasonId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_ReturnsForbidden_WithoutThePermission()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");

        var response = await client.PostAsJsonAsync(
            $"/api/members/{Guid.NewGuid()}/deactivate", new DeactivateMemberRequest { ReasonId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_ReturnsOk_AndTransitionsToInactive_WithThePermission()
    {
        var memberId = await SeedMemberAsync("Dela Cruz", "Juan", "BI-00123");

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Verify);
        await client.PostAsJsonAsync($"/api/members/{memberId}/verify", new VerifyMemberRequest());

        client.DefaultRequestHeaders.Remove(TestAuthHandler.PermissionsHeader);
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var response = await client.PostAsJsonAsync(
            $"/api/members/{memberId}/deactivate",
            new DeactivateMemberRequest { ReasonId = Guid.NewGuid(), Remarks = "Resigned from BI" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var member = await response.Content.ReadFromJsonAsync<MemberDetailResponse>();
        Assert.NotNull(member);
        Assert.Equal("Inactive", member!.Status);
    }

    [Fact]
    public async Task Deactivate_ReturnsConflict_WhenNotActive()
    {
        var memberId = await SeedMemberAsync("Dela Cruz", "Juan", "BI-00123");

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var response = await client.PostAsJsonAsync(
            $"/api/members/{memberId}/deactivate", new DeactivateMemberRequest { ReasonId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Reactivate_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/members/{Guid.NewGuid()}/reactivate", new ReactivateMemberRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Reactivate_ReturnsForbidden_WithoutThePermission()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");

        var response = await client.PostAsJsonAsync($"/api/members/{Guid.NewGuid()}/reactivate", new ReactivateMemberRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Reactivate_ReturnsOk_AndTransitionsToActive_WithThePermission()
    {
        var memberId = await SeedMemberAsync("Dela Cruz", "Juan", "BI-00123");

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Verify);
        await client.PostAsJsonAsync($"/api/members/{memberId}/verify", new VerifyMemberRequest());

        client.DefaultRequestHeaders.Remove(TestAuthHandler.PermissionsHeader);
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);
        await client.PostAsJsonAsync($"/api/members/{memberId}/deactivate", new DeactivateMemberRequest { ReasonId = Guid.NewGuid() });

        var response = await client.PostAsJsonAsync(
            $"/api/members/{memberId}/reactivate", new ReactivateMemberRequest { Remarks = "Rejoined" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var member = await response.Content.ReadFromJsonAsync<MemberDetailResponse>();
        Assert.NotNull(member);
        Assert.Equal("Active", member!.Status);
    }

    [Fact]
    public async Task Reactivate_ReturnsConflict_WhenNotInactive()
    {
        var memberId = await SeedMemberAsync("Dela Cruz", "Juan", "BI-00123");

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var response = await client.PostAsJsonAsync($"/api/members/{memberId}/reactivate", new ReactivateMemberRequest());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private static UpdateMemberRequest CreateValidUpdateRequest()
    {
        return new UpdateMemberRequest
        {
            LastName = "Dela Cruz",
            FirstName = "Juan",
            DateOfBirth = new DateOnly(1990, 1, 1),
            PlaceOfBirth = "Manila",
            CivilStatusId = Guid.NewGuid(),
            PositionDesignation = "Immigration Officer I",
            OfficeUnitId = Guid.NewGuid(),
        };
    }

    private static CreateMemberRequest CreateValidRequest()
    {
        return new CreateMemberRequest
        {
            LastName = "Dela Cruz",
            FirstName = "Juan",
            DateOfBirth = new DateOnly(1990, 1, 1),
            PlaceOfBirth = "Manila",
            CivilStatusId = Guid.NewGuid(),
            EmployeeNumber = "BI-00999",
            PositionDesignation = "Immigration Officer I",
            OfficeUnitId = Guid.NewGuid(),
        };
    }

    private async Task<Guid> SeedMemberAsync(string lastName, string firstName, string employeeNumber)
    {
        await using var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName);

        var member = new Member(
            Guid.NewGuid(), lastName, firstName, middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
            Guid.NewGuid(), joiningReason: null, DateTimeOffset.UtcNow);
        dbContext.Members.Add(member);
        dbContext.MemberEmployments.Add(
            new MemberEmployment(Guid.NewGuid(), member.Id, employeeNumber, "Immigration Officer I", Guid.NewGuid(), null));

        await dbContext.SaveChangesAsync();

        return member.Id;
    }
}
