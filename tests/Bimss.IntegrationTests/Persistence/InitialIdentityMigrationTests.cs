using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Bimss.IntegrationTests.Persistence;

public class InitialIdentityMigrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public Task InitializeAsync() => _sqlContainer.StartAsync();

    public Task DisposeAsync() => _sqlContainer.DisposeAsync().AsTask();

    [Fact]
    public async Task Migrate_AppliesInitialIdentityMigration_CreatesIdentitySchema()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseSqlServer(_sqlContainer.GetConnectionString());

        await using var dbContext = new BimssDbContext(optionsBuilder.Options);

        await dbContext.Database.MigrateAsync();

        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
        Assert.Contains(appliedMigrations, name => name.EndsWith("_InitialIdentity", StringComparison.Ordinal));

        var userCount = await dbContext.Users.CountAsync();
        var roleCount = await dbContext.Roles.CountAsync();

        Assert.Equal(0, userCount);
        Assert.Equal(0, roleCount);
    }
}
