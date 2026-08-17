using System.Security.Claims;
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

    [Theory]
    [InlineData(Permission.Membership.Manage, true)]
    [InlineData(Permission.Membership.ManageSelf, true)]
    [InlineData(Permission.Membership.ViewSelf, false)]
    public async Task ReferenceDataReadPolicy_AllowsManageOrManageSelf_ButNotOtherPermissions(string permission, bool expectedToSucceed)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddBimssAuthorization();
        await using var provider = services.BuildServiceProvider();

        var authorizationService = provider.GetRequiredService<IAuthorizationService>();
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(Permission.ClaimType, permission)], "Test"));

        var result = await authorizationService.AuthorizeAsync(user, resource: null, AuthorizationPolicies.ReferenceDataRead);

        Assert.Equal(expectedToSucceed, result.Succeeded);
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
