using Bimss.Infrastructure.Auditing;
using Bimss.Infrastructure.Identity;
using Bimss.Infrastructure.Membership;
using Bimss.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bimss.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Consolidates the Bimss.Infrastructure-layer registrations
    /// (persistence, Identity, audit logging) that BIMSS-004 through
    /// BIMSS-007 registered piecemeal, so both hosts make one call instead
    /// of three. AddBimssAuthorization() stays a separate top-level call —
    /// see AGENTS.md/the Phase 1 backlog for why.
    /// </summary>
    public static IServiceCollection AddBimssInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBimssPersistence(configuration);
        services.AddBimssIdentity();
        services.AddBimssAuditing();
        services.AddBimssMemberDocumentStorage(configuration);
        services.AddBimssMembership();

        return services;
    }
}
