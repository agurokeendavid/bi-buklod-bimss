using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Bimss.IntegrationTests.Persistence;

public class BimssDbContextConnectivityTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public Task InitializeAsync() => _sqlContainer.StartAsync();

    public Task DisposeAsync() => _sqlContainer.DisposeAsync().AsTask();

    [Fact]
    public async Task CanConnect_ToRealSqlServer_ThroughConfiguredProvider()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseSqlServer(_sqlContainer.GetConnectionString());

        await using var dbContext = new BimssDbContext(optionsBuilder.Options);

        var canConnect = await dbContext.Database.CanConnectAsync();

        Assert.True(canConnect);
    }
}
