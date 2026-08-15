using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Bimss.Infrastructure.Identity;

public sealed class JwtTokenService(IOptions<JwtOptions> options, BimssDbContext dbContext, TimeProvider timeProvider)
{
    public (string AccessToken, DateTimeOffset ExpiresAtUtc) CreateAccessToken(ApplicationUser user)
    {
        var now = timeProvider.GetUtcNow();
        var expires = now.AddMinutes(options.Value.AccessTokenMinutes);

        // sub maps to ClaimTypes.NameIdentifier via JwtBearer's default inbound
        // claim mapping, which is what PermissionClaimsTransformation looks up
        // to re-derive permission claims on every request — no permission
        // claims are embedded here by design.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: options.Value.Issuer,
            audience: options.Value.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public async Task<(string RawToken, DateTimeOffset ExpiresAtUtc)> IssueRefreshTokenAsync(
        Guid userId, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var expires = now.AddDays(options.Value.RefreshTokenDays);
        var rawToken = GenerateRawToken();

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = Hash(rawToken),
            CreatedAtUtc = now,
            ExpiresAtUtc = expires,
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        return (rawToken, expires);
    }

    // Rotation: a validated token is immediately revoked here. The caller is
    // expected to issue a fresh refresh token right after — a reused (already
    // revoked) token is rejected on its next presentation, the standard
    // mitigation for a leaked refresh token being replayed silently.
    public async Task<RefreshToken?> ValidateAndConsumeRefreshTokenAsync(string rawToken, CancellationToken cancellationToken)
    {
        var hash = Hash(rawToken);
        var now = timeProvider.GetUtcNow();

        var existing = await dbContext.RefreshTokens.SingleOrDefaultAsync(token => token.TokenHash == hash, cancellationToken);
        if (existing is null || existing.RevokedAtUtc is not null || existing.ExpiresAtUtc <= now)
        {
            return null;
        }

        existing.RevokedAtUtc = now;
        await dbContext.SaveChangesAsync(cancellationToken);

        return existing;
    }

    public async Task RevokeAsync(string rawToken, CancellationToken cancellationToken)
    {
        var hash = Hash(rawToken);
        var existing = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == hash && token.RevokedAtUtc == null, cancellationToken);
        if (existing is null)
        {
            return;
        }

        existing.RevokedAtUtc = timeProvider.GetUtcNow();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string GenerateRawToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private static string Hash(string rawToken) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
