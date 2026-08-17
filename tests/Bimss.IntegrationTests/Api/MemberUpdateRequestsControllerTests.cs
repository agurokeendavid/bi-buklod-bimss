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

public class MemberUpdateRequestsControllerTests : IDisposable
{
    private static readonly DateTimeOffset OccurredAt = new(2026, 8, 17, 0, 0, 0, TimeSpan.Zero);

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly WebApplicationFactory<Program> _factory;

    public MemberUpdateRequestsControllerTests()
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
    public async Task List_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/update-requests");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_ReturnsForbidden_WithoutThePermission()
    {
        using var client = AuthenticatedClient();

        var response = await client.GetAsync("/api/update-requests");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Reject_ReturnsBadRequest_WhenRemarksAreMissing()
    {
        var requestId = await SeedPendingRequestAsync();
        using var client = AuthenticatedClient(withPermission: true);

        var response = await client.PostAsJsonAsync(
            $"/api/update-requests/{requestId}/reject", new ReviewMemberUpdateRequestRequest { Remarks = null });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Reject_Succeeds_AndDoesNotChangeTheMember()
    {
        var requestId = await SeedPendingRequestAsync();
        using var client = AuthenticatedClient(withPermission: true);

        var response = await client.PostAsJsonAsync(
            $"/api/update-requests/{requestId}/reject", new ReviewMemberUpdateRequestRequest { Remarks = "Name mismatch" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<MemberUpdateRequestDetailResponse>();
        Assert.Equal("Rejected", body!.Status);
        Assert.Equal("Name mismatch", body.ReviewRemarks);
    }

    [Fact]
    public async Task Approve_Succeeds_AndAppliesTheChangeToTheMember()
    {
        var requestId = await SeedPendingRequestAsync();
        using var client = AuthenticatedClient(withPermission: true);

        var response = await client.PostAsJsonAsync(
            $"/api/update-requests/{requestId}/approve", new ReviewMemberUpdateRequestRequest { Remarks = "Confirmed" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<MemberUpdateRequestDetailResponse>();
        Assert.Equal("Approved", body!.Status);

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var member = await readContext.Members.SingleAsync(m => m.Id == body.MemberId);
        Assert.Equal("Juanito", member.FirstName);
    }

    [Fact]
    public async Task List_ThenGetById_RoundTrips()
    {
        var requestId = await SeedPendingRequestAsync();
        using var client = AuthenticatedClient(withPermission: true);

        var listResponse = await client.GetAsync("/api/update-requests?status=Pending");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var summaries = await listResponse.Content.ReadFromJsonAsync<List<MemberUpdateRequestSummaryResponse>>();
        Assert.Single(summaries!);

        var detailResponse = await client.GetAsync($"/api/update-requests/{requestId}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        var detail = await detailResponse.Content.ReadFromJsonAsync<MemberUpdateRequestDetailResponse>();
        Assert.Single(detail!.Changes);
    }

    private HttpClient AuthenticatedClient(bool withPermission = false)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        if (withPermission)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);
        }

        return client;
    }

    private async Task<Guid> SeedPendingRequestAsync()
    {
        await using var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName);

        var member = new Member(
            Guid.NewGuid(), "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
            Guid.NewGuid(), joiningReason: null, OccurredAt);
        dbContext.Members.Add(member);
        dbContext.MemberEmployments.Add(
            new MemberEmployment(Guid.NewGuid(), member.Id, "BI-00123", "Immigration Officer I", Guid.NewGuid(), null));

        var request = new MemberUpdateRequest(
            Guid.NewGuid(), member.Id, Guid.NewGuid(), OccurredAt,
            [new MemberUpdateRequestChangeInput("FirstName", "Juan", "Juanito")]);
        dbContext.MemberUpdateRequests.Add(request);

        await dbContext.SaveChangesAsync();

        return request.Id;
    }
}
