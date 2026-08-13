using System.Net;
using Bimss.Domain.Authorization;

namespace Bimss.IntegrationTests.Api;

public class DiagnosticsAuthorizationTests : IClassFixture<DiagnosticsApiFactory>
{
    private readonly DiagnosticsApiFactory _factory;

    public DiagnosticsAuthorizationTests(DiagnosticsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_ReturnsUnauthorized_WhenRequestIsNotAuthenticated()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/diagnostics/authorized-ping");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsForbidden_WhenAuthenticatedWithoutTheRequiredPermission()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");

        var response = await client.GetAsync("/api/diagnostics/authorized-ping");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsOk_WhenAuthenticatedWithTheRequiredPermission()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.AuthenticatedHeader, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, Permission.Audit.View);

        var response = await client.GetAsync("/api/diagnostics/authorized-ping");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
