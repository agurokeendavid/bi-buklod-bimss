using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.IntegrationTests.Support;

/// <summary>
/// Shared helper for the EF Core InMemory-based test setup (see
/// docs/PHASE1_BACKLOG.md's "Testing convention: EF Core InMemory, not
/// Testcontainers"). Pass a distinct database name per test to isolate its
/// data — a fresh <see cref="Guid.NewGuid()" />-based name per test class
/// instance is the usual pattern.
/// </summary>
public static class InMemoryBimssDbContextFactory
{
    public static BimssDbContext Create(string databaseName)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>()
            .UseInMemoryDatabase(databaseName);

        return new BimssDbContext(optionsBuilder.Options);
    }
}
