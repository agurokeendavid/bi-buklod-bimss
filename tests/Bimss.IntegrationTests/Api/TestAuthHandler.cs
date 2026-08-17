using System.Security.Claims;
using System.Text.Encodings.Web;
using Bimss.Domain.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bimss.IntegrationTests.Api;

public class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string AuthenticatedHeader = "X-Test-Authenticated";
    public const string PermissionsHeader = "X-Test-Permissions";
    public const string UserIdHeader = "X-Test-UserId";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(AuthenticatedHeader, out var authenticatedValue)
            || authenticatedValue != "true")
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = Request.Headers.TryGetValue(UserIdHeader, out var userIdValue) ? userIdValue.ToString() : Guid.NewGuid().ToString();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };

        if (Request.Headers.TryGetValue(PermissionsHeader, out var permissionsValue))
        {
            claims.AddRange(permissionsValue.ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(permission => new Claim(Permission.ClaimType, permission)));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
