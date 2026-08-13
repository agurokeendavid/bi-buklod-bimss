using Bimss.Domain.Authorization;
using Bimss.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Bimss.UnitTests.Authorization;

public class AuthorizationPolicyRegistrationTests
{
    [Fact]
    public async Task AddBimssAuthorization_RegistersAClaimRequiringPolicy_ForEveryCatalogPermission()
    {
        var services = new ServiceCollection();
        services.AddBimssAuthorization();
        await using var provider = services.BuildServiceProvider();

        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        foreach (var permission in Permission.All)
        {
            var policy = await policyProvider.GetPolicyAsync(permission);

            Assert.NotNull(policy);
            Assert.Contains(
                policy.Requirements.OfType<ClaimsAuthorizationRequirement>(),
                requirement => requirement.ClaimType == Permission.ClaimType
                    && requirement.AllowedValues != null
                    && requirement.AllowedValues.Contains(permission));
        }
    }

    [Fact]
    public async Task AddBimssAuthorization_DoesNotRegisterAPolicy_ForAnUnknownPermissionName()
    {
        var services = new ServiceCollection();
        services.AddBimssAuthorization();
        await using var provider = services.BuildServiceProvider();

        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        var policy = await policyProvider.GetPolicyAsync("NotARealPermission");

        Assert.Null(policy);
    }
}
