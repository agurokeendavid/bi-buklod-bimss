using Bimss.Infrastructure.Identity;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bimss.UnitTests.Identity;

public class JwtTokenServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateAccessToken_ReturnsAJwt_WithExpectedExpiry()
    {
        using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "jane.doe" };

        var (accessToken, expiresAtUtc) = service.CreateAccessToken(user);

        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        Assert.Equal(Now.AddMinutes(15), expiresAtUtc);
    }

    [Fact]
    public async Task IssueRefreshTokenAsync_PersistsAHashedToken_NotTheRawValue()
    {
        using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var userId = Guid.NewGuid();

        var (rawToken, expiresAtUtc) = await service.IssueRefreshTokenAsync(userId, CancellationToken.None);

        Assert.Equal(Now.AddDays(14), expiresAtUtc);
        var stored = await dbContext.RefreshTokens.SingleAsync();
        Assert.Equal(userId, stored.UserId);
        Assert.NotEqual(rawToken, stored.TokenHash);
        Assert.Null(stored.RevokedAtUtc);
    }

    [Fact]
    public async Task ValidateAndConsumeRefreshTokenAsync_ReturnsTheToken_AndRevokesIt_WhenValid()
    {
        using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var userId = Guid.NewGuid();
        var (rawToken, _) = await service.IssueRefreshTokenAsync(userId, CancellationToken.None);

        var result = await service.ValidateAndConsumeRefreshTokenAsync(rawToken, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(userId, result!.UserId);
        Assert.Equal(Now, result.RevokedAtUtc);
    }

    [Fact]
    public async Task ValidateAndConsumeRefreshTokenAsync_ReturnsNull_WhenTokenUnknown()
    {
        using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var result = await service.ValidateAndConsumeRefreshTokenAsync("not-a-real-token", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateAndConsumeRefreshTokenAsync_ReturnsNull_WhenAlreadyRevoked()
    {
        using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var (rawToken, _) = await service.IssueRefreshTokenAsync(Guid.NewGuid(), CancellationToken.None);
        await service.ValidateAndConsumeRefreshTokenAsync(rawToken, CancellationToken.None);

        var secondAttempt = await service.ValidateAndConsumeRefreshTokenAsync(rawToken, CancellationToken.None);

        Assert.Null(secondAttempt);
    }

    [Fact]
    public async Task RevokeAsync_MarksTheTokenRevoked()
    {
        using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var (rawToken, _) = await service.IssueRefreshTokenAsync(Guid.NewGuid(), CancellationToken.None);

        await service.RevokeAsync(rawToken, CancellationToken.None);

        var stored = await dbContext.RefreshTokens.SingleAsync();
        Assert.NotNull(stored.RevokedAtUtc);
    }

    private static JwtTokenService CreateService(BimssDbContext dbContext)
    {
        var options = Options.Create(new JwtOptions
        {
            SigningKey = "unit-test-signing-key-never-a-real-secret-0123456789ABCDEF",
            Issuer = "Bimss",
            Audience = "Bimss",
            AccessTokenMinutes = 15,
            RefreshTokenDays = 14,
        });

        return new JwtTokenService(options, dbContext, new FixedTimeProvider(Now));
    }

    private static BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        return new BimssDbContext(optionsBuilder.Options);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
