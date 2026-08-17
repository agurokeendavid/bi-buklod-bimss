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

public class MyUpdateRequestsControllerTests : IDisposable
{
    private static readonly DateTimeOffset OccurredAt = new(2026, 8, 17, 0, 0, 0, TimeSpan.Zero);

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly WebApplicationFactory<Program> _factory;

    public MyUpdateRequestsControllerTests()
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
    public async Task Submit_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/my/update-requests", BuildRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Submit_ReturnsForbidden_WithoutManageSelfPermission()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");

        var response = await client.PostAsJsonAsync("/api/my/update-requests", BuildRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Submit_ReturnsNotFound_WhenTheCallerHasNoLinkedMember()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.ManageSelf);

        var response = await client.PostAsJsonAsync("/api/my/update-requests", BuildRequest());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Submit_ReturnsBadRequest_WhenNoFieldsChanged()
    {
        var userId = Guid.NewGuid();
        Member member = null!;
        MemberEmployment employment = null!;

        await using (var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            (member, employment) = await SeedMemberAsync(dbContext, userId);
        }

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.ManageSelf);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());

        var response = await client.PostAsJsonAsync("/api/my/update-requests", BuildRequest(member, employment));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Submit_Succeeds_AndPersistsTheChange()
    {
        var userId = Guid.NewGuid();
        Member member = null!;
        MemberEmployment employment = null!;

        await using (var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName))
        {
            (member, employment) = await SeedMemberAsync(dbContext, userId);
        }

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.ManageSelf);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());

        var request = BuildRequest(member, employment);
        request.FirstName = "Juanito";
        var response = await client.PostAsJsonAsync("/api/my/update-requests", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SubmitMemberUpdateRequestResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.Id);

        await using var readContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        var persisted = await readContext.MemberUpdateRequests.SingleAsync(r => r.Id == body.Id);
        Assert.Equal(member.Id, persisted.MemberId);
        Assert.Equal(MemberUpdateRequestStatus.Pending, persisted.Status);
    }

    private static async Task<(Member Member, MemberEmployment Employment)> SeedMemberAsync(BimssDbContext dbContext, Guid userId)
    {
        var member = new Member(
            Guid.NewGuid(), "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
            Guid.NewGuid(), joiningReason: null, OccurredAt);
        var employment = new MemberEmployment(Guid.NewGuid(), member.Id, "BI-00123", "Immigration Officer I", Guid.NewGuid(), null);

        dbContext.Members.Add(member);
        dbContext.MemberEmployments.Add(employment);
        dbContext.Users.Add(new ApplicationUser { Id = userId, UserName = "member.dev", MemberId = member.Id });

        await dbContext.SaveChangesAsync();

        return (member, employment);
    }

    private static UpdateMemberRequest BuildRequest(Member? member = null, MemberEmployment? employment = null)
    {
        return new UpdateMemberRequest
        {
            LastName = member?.LastName ?? "Dela Cruz",
            FirstName = member?.FirstName ?? "Juan",
            MiddleName = member?.MiddleName,
            SuffixId = member?.SuffixId,
            DateOfBirth = member?.DateOfBirth ?? new DateOnly(1990, 1, 1),
            PlaceOfBirth = member?.PlaceOfBirth ?? "Manila",
            CivilStatusId = member?.CivilStatusId ?? Guid.NewGuid(),
            JoiningReason = member?.JoiningReason,
            PositionDesignation = employment?.PositionDesignation ?? "Immigration Officer I",
            OfficeUnitId = employment?.OfficeUnitId ?? Guid.NewGuid(),
            PermanentAppointmentDate = employment?.PermanentAppointmentDate,
        };
    }
}
