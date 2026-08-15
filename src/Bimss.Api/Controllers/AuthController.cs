using Bimss.Contracts.Auth;
using Bimss.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bimss.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    JwtTokenService jwtTokenService) : ControllerBase
{
    // httpOnly so client-side JS can never read the refresh token; Secure
    // requires HTTPS (see launchSettings.json's https profile for local
    // dev); SameSite=None is required since the frontend and Bimss.Api are
    // different origins. Known, accepted tradeoff for this phase: an
    // attacker triggering /api/auth/refresh cross-site can rotate a
    // legitimate user's token without being able to read the response
    // (blocked by CORS), causing at most a wasted rotation — not token
    // theft. Documented in docs/SECURITY_AND_PRIVACY.md.
    private const string RefreshTokenCookieName = "bimss_refresh_token";

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByNameAsync(request.UserName);
        if (user is null)
        {
            return InvalidCredentials();
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return InvalidCredentials();
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var rawToken) || string.IsNullOrEmpty(rawToken))
        {
            return Unauthorized();
        }

        var stored = await jwtTokenService.ValidateAndConsumeRefreshTokenAsync(rawToken, cancellationToken);
        if (stored is null)
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null)
        {
            return Unauthorized();
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(RefreshTokenCookieName, out var rawToken) && !string.IsNullOrEmpty(rawToken))
        {
            await jwtTokenService.RevokeAsync(rawToken, cancellationToken);
        }

        Response.Cookies.Delete(RefreshTokenCookieName);

        return NoContent();
    }

    private IActionResult InvalidCredentials() =>
        Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid username or password.");

    private async Task<IActionResult> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var (accessToken, accessExpires) = jwtTokenService.CreateAccessToken(user);
        var (refreshToken, refreshExpires) = await jwtTokenService.IssueRefreshTokenAsync(user.Id, cancellationToken);

        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = refreshExpires,
        });

        return Ok(new LoginResponse { AccessToken = accessToken, ExpiresAtUtc = accessExpires });
    }
}
