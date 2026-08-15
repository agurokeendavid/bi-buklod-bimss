using Bimss.Application.Membership;
using Microsoft.Extensions.DependencyInjection;

namespace Bimss.Infrastructure.Membership;

public static class MembershipServiceCollectionExtensions
{
    public static IServiceCollection AddBimssMembership(this IServiceCollection services)
    {
        services.AddScoped<IMemberRepository, MemberRepository>();

        return services;
    }
}
