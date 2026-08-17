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

public class ImportBatchesControllerTests : IDisposable
{
    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly WebApplicationFactory<Program> _factory;

    public ImportBatchesControllerTests()
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

        var response = await client.GetAsync("/api/import-batches");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_ReturnsForbidden_WithoutThePermission()
    {
        using var client = AuthenticatedClient();

        var response = await client.GetAsync("/api/import-batches");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Ingest_ReturnsBadRequest_WhenNoFileIsProvided()
    {
        using var client = AuthenticatedClient(withPermission: true);
        using var content = new MultipartFormDataContent();

        var response = await client.PostAsync("/api/import-batches", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Ingest_ThenList_ThenGetById_RoundTrips()
    {
        using var client = AuthenticatedClient(withPermission: true);

        var workbookBytes = ExcelFixtures.BuildWorkbook(
            headers: ["Last Name", "First Name"],
            rows: [["Dela Cruz", "Juan"], ["Santos", "Ana"]]);
        var fileContent = new ByteArrayContent(workbookBytes);
        using var uploadContent = new MultipartFormDataContent { { fileContent, "file", "legacy-members.xlsx" } };

        var ingestResponse = await client.PostAsync("/api/import-batches", uploadContent);
        Assert.Equal(HttpStatusCode.Created, ingestResponse.StatusCode);
        var ingested = await ingestResponse.Content.ReadFromJsonAsync<ImportBatchIngestResponse>();
        Assert.NotNull(ingested);
        Assert.Equal(2, ingested!.RowCount);

        var listResponse = await client.GetAsync("/api/import-batches");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var batches = await listResponse.Content.ReadFromJsonAsync<List<ImportBatchSummaryResponse>>();
        var batch = Assert.Single(batches!);
        Assert.Equal("legacy-members.xlsx", batch.FileName);
        Assert.Equal("Staged", batch.Status);

        var detailResponse = await client.GetAsync($"/api/import-batches/{ingested.Id}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        var detail = await detailResponse.Content.ReadFromJsonAsync<ImportBatchDetailResponse>();
        Assert.Equal("Staged", detail!.Status);

        var rowsResponse = await client.GetAsync($"/api/import-batches/{ingested.Id}/rows");
        Assert.Equal(HttpStatusCode.OK, rowsResponse.StatusCode);
        var rows = await rowsResponse.Content.ReadFromJsonAsync<List<MemberImportStagingRowResponse>>();
        Assert.Equal(2, rows!.Count);
        Assert.Equal("Dela Cruz", rows[0].LastName);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenBatchDoesNotExist()
    {
        using var client = AuthenticatedClient(withPermission: true);

        var response = await client.GetAsync($"/api/import-batches/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Validate_ThenMatch_TransitionsBatchStatus()
    {
        using var client = AuthenticatedClient(withPermission: true);

        var workbookBytes = ExcelFixtures.BuildWorkbook(
            headers: ["Last Name", "First Name", "Place Of Birth", "Date Of Birth", "Civil Status", "BI Employee Number", "Position/Designation", "Division/Section/Unit"],
            rows: [["Dela Cruz", "Juan", "Manila", "1990-01-15", "Single", "BI-00123", "Officer I", "Port Operations Division"]]);
        var fileContent = new ByteArrayContent(workbookBytes);
        using var uploadContent = new MultipartFormDataContent { { fileContent, "file", "legacy-members.xlsx" } };
        var ingestResponse = await client.PostAsync("/api/import-batches", uploadContent);
        var ingested = await ingestResponse.Content.ReadFromJsonAsync<ImportBatchIngestResponse>();

        await SeedReferenceDataAsync();

        var validateResponse = await client.PostAsync($"/api/import-batches/{ingested!.Id}/validate", content: null);
        Assert.Equal(HttpStatusCode.OK, validateResponse.StatusCode);
        var validated = await validateResponse.Content.ReadFromJsonAsync<ImportBatchDetailResponse>();
        Assert.Equal("Validated", validated!.Status);

        var matchResponse = await client.PostAsync($"/api/import-batches/{ingested.Id}/match", content: null);
        Assert.Equal(HttpStatusCode.OK, matchResponse.StatusCode);

        var rowsResponse = await client.GetAsync($"/api/import-batches/{ingested.Id}/rows");
        var rows = await rowsResponse.Content.ReadFromJsonAsync<List<MemberImportStagingRowResponse>>();
        var row = Assert.Single(rows!);
        Assert.Equal("Valid", row.ValidationStatus);
        Assert.Equal("NoMatch", row.MatchStatus);
    }

    [Fact]
    public async Task PromoteRow_CreatesAMember_ForAnEligibleRow()
    {
        using var client = AuthenticatedClient(withPermission: true);

        var workbookBytes = ExcelFixtures.BuildWorkbook(
            headers: ["Last Name", "First Name", "Place Of Birth", "Date Of Birth", "Civil Status", "BI Employee Number", "Position/Designation", "Division/Section/Unit"],
            rows: [["Dela Cruz", "Juan", "Manila", "1990-01-15", "Single", "BI-00123", "Officer I", "Port Operations Division"]]);
        var fileContent = new ByteArrayContent(workbookBytes);
        using var uploadContent = new MultipartFormDataContent { { fileContent, "file", "legacy-members.xlsx" } };
        var ingestResponse = await client.PostAsync("/api/import-batches", uploadContent);
        var ingested = await ingestResponse.Content.ReadFromJsonAsync<ImportBatchIngestResponse>();

        await SeedReferenceDataAsync();
        await client.PostAsync($"/api/import-batches/{ingested!.Id}/validate", content: null);
        await client.PostAsync($"/api/import-batches/{ingested.Id}/match", content: null);

        var rowsResponse = await client.GetAsync($"/api/import-batches/{ingested.Id}/rows");
        var rows = await rowsResponse.Content.ReadFromJsonAsync<List<MemberImportStagingRowResponse>>();
        var rowId = rows!.Single().Id;

        var promoteResponse = await client.PostAsync($"/api/import-batches/{ingested.Id}/rows/{rowId}/promote", content: null);
        Assert.Equal(HttpStatusCode.OK, promoteResponse.StatusCode);
        var promoted = await promoteResponse.Content.ReadFromJsonAsync<PromoteImportRowResponse>();
        Assert.NotEqual(Guid.Empty, promoted!.MemberId);

        await using var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        Assert.True(await dbContext.Members.AnyAsync(m => m.Id == promoted.MemberId));
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

    private async Task SeedReferenceDataAsync()
    {
        await using var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName);
        dbContext.CivilStatuses.Add(new CivilStatus(Guid.NewGuid(), "SGL", "Single"));
        dbContext.OfficeUnits.Add(new OfficeUnit(Guid.NewGuid(), "POD", "Port Operations Division"));
        await dbContext.SaveChangesAsync();
    }
}
