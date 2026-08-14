extern alias WebHost;

using System.Net;
using System.Text.RegularExpressions;
using Bimss.Infrastructure.Identity;
using Bimss.Infrastructure.Persistence;
using Bimss.IntegrationTests.Support;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebProgram = WebHost::Program;

namespace Bimss.IntegrationTests.Web;

public class LoginTests : IDisposable
{
    private const string UserName = "login.test.user";
    private const string Password = "Correct-Horse-42!";

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly WebApplicationFactory<WebProgram> _factory;

    public LoginTests()
    {
        _factory = new WebApplicationFactory<WebProgram>().WithWebHostBuilder(builder =>
        {
            // Avoid WebApplicationFactory's default "Development" environment
            // triggering DevelopmentIdentitySeeder — this test seeds its own
            // single user and doesn't need the six dev accounts too.
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<BimssDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<BimssDbContext>>();
                services.AddDbContext<BimssDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            });
        });

        SeedUser();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    [Fact]
    public async Task Login_ThenLogout_RoundTripsTheSession()
    {
        using var client = _factory.CreateClient();

        var loginPage = await client.GetAsync("/Account/Login");
        Assert.Equal(HttpStatusCode.OK, loginPage.StatusCode);
        var token = ExtractAntiForgeryToken(await loginPage.Content.ReadAsStringAsync());

        var loginResponse = await client.PostAsync(
            "/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["UserName"] = UserName,
                ["Password"] = Password,
                ["__RequestVerificationToken"] = token,
            }));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var homeHtml = await loginResponse.Content.ReadAsStringAsync();
        Assert.Contains($"Hello, {UserName}!", homeHtml);

        var logoutToken = ExtractAntiForgeryToken(homeHtml);
        var logoutResponse = await client.PostAsync(
            "/Account/Logout",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = logoutToken,
            }));

        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
        var homeAfterLogoutHtml = await logoutResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain($"Hello, {UserName}!", homeAfterLogoutHtml);
    }

    [Fact]
    public async Task Login_ShowsValidationError_ForWrongPassword()
    {
        using var client = _factory.CreateClient();

        var loginPage = await client.GetAsync("/Account/Login");
        var token = ExtractAntiForgeryToken(await loginPage.Content.ReadAsStringAsync());

        var response = await client.PostAsync(
            "/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["UserName"] = UserName,
                ["Password"] = "wrong-password",
                ["__RequestVerificationToken"] = token,
            }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid username or password.", html);
    }

    private void SeedUser()
    {
        using var dbContext = CreateDbContext();

        var user = new ApplicationUser
        {
            UserName = UserName,
            NormalizedUserName = UserName.ToUpperInvariant(),
            Email = "login.test.user@example.test",
            NormalizedEmail = "LOGIN.TEST.USER@EXAMPLE.TEST",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("N"),
        };
        user.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(user, Password);
        dbContext.Users.Add(user);
        dbContext.SaveChanges();
    }

    private BimssDbContext CreateDbContext() => InMemoryBimssDbContextFactory.Create(_databaseName);

    private static string ExtractAntiForgeryToken(string html)
    {
        var inputTag = Regex.Match(html, "<input[^>]*name=\"__RequestVerificationToken\"[^>]*>").Value;
        var valueMatch = Regex.Match(inputTag, "value=\"([^\"]*)\"");

        return valueMatch.Success ? valueMatch.Groups[1].Value : string.Empty;
    }
}
