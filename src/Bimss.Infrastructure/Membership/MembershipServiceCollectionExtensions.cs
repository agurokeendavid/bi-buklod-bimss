using Bimss.Application.Membership;
using Microsoft.Extensions.DependencyInjection;

namespace Bimss.Infrastructure.Membership;

public static class MembershipServiceCollectionExtensions
{
    public static IServiceCollection AddBimssMembership(this IServiceCollection services)
    {
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IMemberQueryService, MemberQueryService>();

        return services;
    }
}
