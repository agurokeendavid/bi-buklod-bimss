using System.Net;
using System.Net.Http.Json;
using Bimss.Application.Membership;
using Bimss.Contracts.Membership;
using Bimss.Domain.Authorization;
using Bimss.Domain.Membership;
using Bimss.Infrastructure.Membership;
using Bimss.Infrastructure.Persistence;
using Bimss.IntegrationTests.Support;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Bimss.IntegrationTests.Api;

public class MemberDocumentsControllerTests : IDisposable
{
    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly string _documentStorageRootPath = Path.Combine(Path.GetTempPath(), "bimss-tests", Guid.NewGuid().ToString());
    private readonly WebApplicationFactory<Program> _factory;

    public MemberDocumentsControllerTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<BimssDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<BimssDbContext>>();
                services.AddDbContext<BimssDbContext>(options => options.UseInMemoryDatabase(_databaseName));

                services.RemoveAll<IMemberDocumentStorage>();
                services.AddSingleton<IMemberDocumentStorage>(new LocalFileMemberDocumentStorage(
                    Options.Create(new MemberDocumentStorageOptions { RootPath = _documentStorageRootPath })));

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

        if (Directory.Exists(_documentStorageRootPath))
        {
            Directory.Delete(_documentStorageRootPath, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task List_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/members/{Guid.NewGuid()}/documents");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_ReturnsForbidden_WithoutThePermission()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");

        var response = await client.GetAsync($"/api/members/{Guid.NewGuid()}/documents");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Upload_ReturnsBadRequest_WhenExtensionIsNotAccepted()
    {
        var memberId = await SeedMemberAsync();

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        using var content = new MultipartFormDataContent
        {
            { new ByteArrayContent([1, 2, 3]), "file", "malware.exe" },
            { new StringContent("ProofOfEmployment"), "documentType" },
        };

        var response = await client.PostAsync($"/api/members/{memberId}/documents", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_ThenList_ThenDownload_RoundTrips_WithThePermission()
    {
        var memberId = await SeedMemberAsync();

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var originalBytes = "synthetic proof of employment"u8.ToArray();
        var fileContent = new ByteArrayContent(originalBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        using var uploadContent = new MultipartFormDataContent
        {
            { fileContent, "file", "coe.pdf" },
            { new StringContent("ProofOfEmployment"), "documentType" },
        };

        var uploadResponse = await client.PostAsync($"/api/members/{memberId}/documents", uploadContent);
        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);

        var listResponse = await client.GetAsync($"/api/members/{memberId}/documents");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var documents = await listResponse.Content.ReadFromJsonAsync<List<MemberDocumentSummaryResponse>>();
        Assert.NotNull(documents);
        var document = Assert.Single(documents!);
        Assert.Equal("ProofOfEmployment", document.DocumentType);
        Assert.Equal("coe.pdf", document.OriginalFileName);
        Assert.Equal(originalBytes.Length, document.FileSizeBytes);

        var downloadResponse = await client.GetAsync($"/api/members/{memberId}/documents/{document.Id}/download");
        Assert.Equal(HttpStatusCode.OK, downloadResponse.StatusCode);
        var downloadedBytes = await downloadResponse.Content.ReadAsByteArrayAsync();
        Assert.Equal(originalBytes, downloadedBytes);
    }

    [Fact]
    public async Task Download_ReturnsNotFound_WhenDocumentDoesNotExist()
    {
        var memberId = await SeedMemberAsync();

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Membership.Manage);

        var response = await client.GetAsync($"/api/members/{memberId}/documents/{Guid.NewGuid()}/download");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<Guid> SeedMemberAsync()
    {
        await using var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName);

        var member = new Member(
            Guid.NewGuid(), "Dela Cruz", "Juan", middleName: null, suffixId: null, new DateOnly(1990, 1, 1), "Manila",
            Guid.NewGuid(), joiningReason: null, DateTimeOffset.UtcNow);
        dbContext.Members.Add(member);
        dbContext.MemberEmployments.Add(
            new MemberEmployment(Guid.NewGuid(), member.Id, "BI-00123", "Immigration Officer I", Guid.NewGuid(), null));

        await dbContext.SaveChangesAsync();

        return member.Id;
    }
}
