using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bimss.Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddBimssPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BimssDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Bimss")));

        return services;
    }
}
