using Bimss.Domain.Authorization;
using Bimss.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Bimss.Infrastructure.Authorization;

public static class AuthorizationServiceCollectionExtensions
{
    public static IServiceCollection AddBimssAuthorization(this IServiceCollection services)
    {
        services.AddScoped<IClaimsTransformation, PermissionClaimsTransformation>();

        services.AddAuthorization(options =>
        {
            foreach (var permission in Permission.All)
            {
                options.AddPolicy(permission, policy => policy.RequireClaim(Permission.ClaimType, permission));
            }
        });

        return services;
    }
}
