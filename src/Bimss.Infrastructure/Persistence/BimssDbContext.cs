using Microsoft.EntityFrameworkCore;

namespace Bimss.Infrastructure.Persistence;

public class BimssDbContext(DbContextOptions<BimssDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BimssDbContext).Assembly);
    }
}
