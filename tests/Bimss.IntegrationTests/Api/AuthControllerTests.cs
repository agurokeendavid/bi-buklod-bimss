using System.Net;
using System.Net.Http.Json;
using Bimss.Contracts.Auth;
using Bimss.Infrastructure.Identity;
using Bimss.Infrastructure.Persistence;
using Bimss.IntegrationTests.Support;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bimss.IntegrationTests.Api;

public class AuthControllerTests : IDisposable
{
    private const string UserName = "auth.test.user";
    private const string Password = "Correct-Horse-99!";
    private const string RefreshCookieName = "bimss_refresh_token";

    // Test-only signing key — never a real secret, only ever used inside this
    // in-memory test host.
    private const string SigningKey = "test-only-signing-key-never-a-real-secret-0123456789ABCDEF";

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly WebApplicationFactory<Program> _factory;

    public AuthControllerTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            // Avoid WebApplicationFactory's default "Development" environment
            // triggering the development seeders — this test seeds its own
            // single user.
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SigningKey"] = SigningKey,
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<BimssDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<BimssDbContext>>();
                services.AddDbContext<BimssDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            });
        });

        SeedUser();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Login_Succeeds_WithValidCredentials_AndSetsRefreshCookie()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { UserName = UserName, Password = Password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(ExtractCookieValue(response, RefreshCookieName)));
    }

    [Fact]
    public async Task Login_Fails_WithInvalidPassword()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { UserName = UserName, Password = "wrong-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_Fails_WithUnknownUsername_AndReturnsTheSameGenericMessage()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { UserName = "nobody.at.all", Password = Password });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid username or password.", body);
    }

    [Fact]
    public async Task Refresh_IssuesANewTokenPair()
    {
        using var client = CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { UserName = UserName, Password = Password });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        var cookieValue = ExtractCookieValue(loginResponse, RefreshCookieName);

        var refreshResponse = await client.SendAsync(CreateRequestWithCookie(HttpMethod.Post, "/api/auth/refresh", cookieValue));

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var refreshBody = await refreshResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(refreshBody);
        Assert.NotEqual(loginBody!.AccessToken, refreshBody!.AccessToken);
        Assert.NotEqual(cookieValue, ExtractCookieValue(refreshResponse, RefreshCookieName));
    }

    [Fact]
    public async Task Refresh_Fails_WhenTheSameTokenIsReusedAfterRotation()
    {
        using var client = CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { UserName = UserName, Password = Password });
        var originalCookieValue = ExtractCookieValue(loginResponse, RefreshCookieName);

        var firstRefresh = await client.SendAsync(CreateRequestWithCookie(HttpMethod.Post, "/api/auth/refresh", originalCookieValue));
        Assert.Equal(HttpStatusCode.OK, firstRefresh.StatusCode);

        var reuseResponse = await client.SendAsync(CreateRequestWithCookie(HttpMethod.Post, "/api/auth/refresh", originalCookieValue));

        Assert.Equal(HttpStatusCode.Unauthorized, reuseResponse.StatusCode);
    }

    [Fact]
    public async Task Refresh_Fails_WhenNoCookieIsPresent()
    {
        using var client = CreateClient();

        var response = await client.PostAsync("/api/auth/refresh", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesTheToken_SoItCanNoLongerBeRefreshed()
    {
        using var client = CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { UserName = UserName, Password = Password });
        var cookieValue = ExtractCookieValue(loginResponse, RefreshCookieName);

        var logoutResponse = await client.SendAsync(CreateRequestWithCookie(HttpMethod.Post, "/api/auth/logout", cookieValue));
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        var refreshAfterLogout = await client.SendAsync(CreateRequestWithCookie(HttpMethod.Post, "/api/auth/refresh", cookieValue));

        Assert.Equal(HttpStatusCode.Unauthorized, refreshAfterLogout.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_ReturnsUnauthorized_WithNoAccessToken()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/diagnostics/authorized-ping");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_ReturnsForbidden_WithAValidAccessToken_ButNoPermission()
    {
        using var client = CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { UserName = UserName, Password = Password });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/diagnostics/authorized-ping");
        request.Headers.Add("Authorization", $"Bearer {loginBody!.AccessToken}");

        var response = await client.SendAsync(request);

        // Proves the JWT itself authenticated successfully (principal
        // recognized) — it fails on the permission policy, not on the token.
        // Full role/permission-driven 200 coverage already exists in
        // DiagnosticsAuthorizationTests via TestAuthHandler.
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private void SeedUser()
    {
        using var dbContext = InMemoryBimssDbContextFactory.Create(_databaseName);

        var user = new ApplicationUser
        {
            UserName = UserName,
            NormalizedUserName = UserName.ToUpperInvariant(),
            Email = "auth.test.user@example.test",
            NormalizedEmail = "AUTH.TEST.USER@EXAMPLE.TEST",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("N"),
        };
        user.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(user, Password);
        dbContext.Users.Add(user);
        dbContext.SaveChanges();
    }

    // Cookie handling is disabled so every test controls the Cookie header
    // explicitly via CreateRequestWithCookie/ExtractCookieValue rather than
    // relying on WebApplicationFactory's default automatic cookie container,
    // which would make testing rotation/reuse-rejection ambiguous.
    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });

    private static HttpRequestMessage CreateRequestWithCookie(HttpMethod method, string url, string cookieValue)
    {
        var request = new HttpRequestMessage(method, url);
        if (!string.IsNullOrEmpty(cookieValue))
        {
            request.Headers.Add("Cookie", $"{RefreshCookieName}={cookieValue}");
        }

        return request;
    }

    private static string ExtractCookieValue(HttpResponseMessage response, string cookieName)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
        {
            return string.Empty;
        }

        foreach (var setCookie in setCookieValues)
        {
            if (!setCookie.StartsWith($"{cookieName}=", StringComparison.Ordinal))
            {
                continue;
            }

            var separatorIndex = setCookie.IndexOf(';');
            var pair = separatorIndex >= 0 ? setCookie[..separatorIndex] : setCookie;
            return pair[(cookieName.Length + 1)..];
        }

        return string.Empty;
    }
}
