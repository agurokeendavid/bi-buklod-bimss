using System.Net;
using System.Net.Http.Json;
using Bimss.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bimss.IntegrationTests.Api;

public class ExceptionHandlingTests : IClassFixture<DiagnosticsApiFactory>
{
    private readonly DiagnosticsApiFactory _factory;

    public ExceptionHandlingTests(DiagnosticsApiFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("notfound", HttpStatusCode.NotFound)]
    [InlineData("conflict", HttpStatusCode.Conflict)]
    [InlineData("forbidden", HttpStatusCode.Forbidden)]
    [InlineData("validation", HttpStatusCode.BadRequest)]
    public async Task Get_MapsTypedException_ToTheCorrectStatusCode(string exceptionType, HttpStatusCode expectedStatus)
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/diagnostics/throw?type={exceptionType}");

        Assert.Equal(expectedStatus, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal((int)expectedStatus, problemDetails.Status);
    }

    [Fact]
    public async Task Get_MapsAnUnexpectedException_To500_WithoutLeakingInternalDetails_InProduction()
    {
        using var productionClient = _factory
            .WithWebHostBuilder(builder => builder.UseEnvironment("Production"))
            .CreateClient();

        var response = await productionClient.GetAsync("/api/diagnostics/throw?type=unexpected");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.DoesNotContain("hunter2", body);
        Assert.DoesNotContain("InvalidOperationException", body);
        Assert.DoesNotContain("Bimss.Api.Controllers.DiagnosticsController", body);
    }

    [Fact]
    public async Task Get_IncludesExceptionDetails_InDevelopment()
    {
        // Development triggers DevelopmentIdentitySeeder on startup, so this
        // (unlike the base DiagnosticsApiFactory, which uses the "Testing"
        // environment to avoid that) needs a real DbContext for it to seed
        // into — InMemory, same pattern as LoginTests.
        var databaseName = Guid.NewGuid().ToString();
        using var developmentClient = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<DbContextOptions<BimssDbContext>>();
                    services.RemoveAll<IDbContextOptionsConfiguration<BimssDbContext>>();
                    services.AddDbContext<BimssDbContext>(options => options.UseInMemoryDatabase(databaseName));
                });
            })
            .CreateClient();

        var response = await developmentClient.GetAsync("/api/diagnostics/throw?type=unexpected");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("InvalidOperationException", body);
    }
}
