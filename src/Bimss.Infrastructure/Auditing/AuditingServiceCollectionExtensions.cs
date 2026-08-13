using Bimss.Application.Auditing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bimss.Infrastructure.Auditing;

public static class AuditingServiceCollectionExtensions
{
    public static IServiceCollection AddBimssAuditing(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IAuditLogger, AuditLogger>();

        return services;
    }
}
