using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Bimss.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef` design-time tooling construct a <see cref="BimssDbContext"/> without
/// running the full application host. Not used at application runtime.
/// </summary>
public class BimssDbContextFactory : IDesignTimeDbContextFactory<BimssDbContext>
{
    public BimssDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Bimss")
            ?? "Server=(localdb)\\mssqllocaldb;Database=BimssDesignTime;Trusted_Connection=True;MultipleActiveResultSets=true";

        var optionsBuilder = new DbContextOptionsBuilder<BimssDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new BimssDbContext(optionsBuilder.Options);
    }
}
