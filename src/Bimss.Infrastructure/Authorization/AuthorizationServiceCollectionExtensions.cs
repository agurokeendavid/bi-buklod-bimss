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

            // Reference/master data (civil statuses, suffixes, office units,
            // ...) isn't member-specific or sensitive — it's shared taxonomy
            // both the officer-facing admin forms and the member
            // self-service edit form (BIMSS-042) need to populate a Select
            // from. A single [Authorize(Policy = Permission.Membership.X)]
            // can't express "either of these permissions" (stacking
            // attributes AND-combines them, per MembersController's own
            // comment on the same pitfall), so this is a named policy
            // rather than a 1:1 Permission entry.
            options.AddPolicy(AuthorizationPolicies.ReferenceDataRead, policy => policy.RequireAssertion(context =>
                context.User.HasClaim(Permission.ClaimType, Permission.Membership.Manage)
                || context.User.HasClaim(Permission.ClaimType, Permission.Membership.ManageSelf)));
        });

        return services;
    }
}
