using Bimss.Application.Auditing;
using Bimss.Domain.Auditing;
using Bimss.Infrastructure.Auditing;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Bimss.IntegrationTests.Auditing;

public class AuditLoggerTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _sqlContainer.DisposeAsync().AsTask();

    [Fact]
    public async Task LogAsync_RoundTripsAFullAuditEntry_ThroughPersistence()
    {
        var actorUserId = Guid.NewGuid();
        var fixedTime = new DateTimeOffset(2026, 1, 15, 9, 30, 0, TimeSpan.Zero);
        var entry = new AuditEntry(
            actorUserId: actorUserId,
            action: "Member.Verify",
            objectType: "Member",
            objectId: "member-0001",
            result: AuditResult.Success,
            remarks: "Verified against synthetic BI employment records.",
            metadata: new Dictionary<string, string> { ["previousStatus"] = "Pending", ["newStatus"] = "Verified" });

        await using (var writeContext = CreateDbContext())
        {
            var logger = new AuditLogger(writeContext, new FakeTimeProvider(fixedTime));
            await logger.LogAsync(entry);
        }

        await using var readContext = CreateDbContext();
        var persisted = await readContext.AuditEvents.SingleAsync();

        Assert.NotEqual(Guid.Empty, persisted.Id);
        Assert.Equal(actorUserId, persisted.ActorUserId);
        Assert.Equal("Member.Verify", persisted.Action);
        Assert.Equal("Member", persisted.ObjectType);
        Assert.Equal("member-0001", persisted.ObjectId);
        Assert.Equal(fixedTime, persisted.TimestampUtc);
        Assert.Equal(AuditResult.Success, persisted.Result);
        Assert.Equal("Verified against synthetic BI employment records.", persisted.Remarks);
        Assert.Contains("\"previousStatus\":\"Pending\"", persisted.MetadataJson);
        Assert.Contains("\"newStatus\":\"Verified\"", persisted.MetadataJson);
    }

    [Fact]
    public async Task LogAsync_PersistsAFailureResult_WithNoActorOrMetadata()
    {
        var entry = new AuditEntry(
            actorUserId: null,
            action: "Election.Finalize",
            objectType: "Election",
            objectId: "election-2026",
            result: AuditResult.Failure,
            remarks: null,
            metadata: null);

        await using (var writeContext = CreateDbContext())
        {
            var logger = new AuditLogger(writeContext, TimeProvider.System);
            await logger.LogAsync(entry);
        }

        await using var readContext = CreateDbContext();
        var persisted = await readContext.AuditEvents.SingleAsync();

        Assert.Null(persisted.ActorUserId);
        Assert.Equal(AuditResult.Failure, persisted.Result);
        Assert.Null(persisted.Remarks);
        Assert.Null(persisted.MetadataJson);
    }

    private BimssDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseSqlServer(_sqlContainer.GetConnectionString());

        return new BimssDbContext(optionsBuilder.Options);
    }

    private sealed class FakeTimeProvider(DateTimeOffset fixedTime) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => fixedTime;
    }
}
