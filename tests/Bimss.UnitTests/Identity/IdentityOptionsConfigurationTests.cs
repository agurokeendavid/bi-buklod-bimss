using Bimss.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Bimss.UnitTests.Identity;

public class IdentityOptionsConfigurationTests
{
    [Fact]
    public void ConfigureIdentityOptions_TightensPasswordPolicy_BeyondFrameworkDefaults()
    {
        var options = new IdentityOptions();

        IdentityServiceCollectionExtensions.ConfigureIdentityOptions(options);

        Assert.Equal(12, options.Password.RequiredLength);
        Assert.True(options.Password.RequireDigit);
        Assert.True(options.Password.RequireLowercase);
        Assert.True(options.Password.RequireUppercase);
        Assert.True(options.Password.RequireNonAlphanumeric);
        Assert.Equal(4, options.Password.RequiredUniqueChars);
    }

    [Fact]
    public void ConfigureIdentityOptions_TightensLockoutPolicy()
    {
        var options = new IdentityOptions();

        IdentityServiceCollectionExtensions.ConfigureIdentityOptions(options);

        Assert.Equal(5, options.Lockout.MaxFailedAccessAttempts);
        Assert.Equal(TimeSpan.FromMinutes(15), options.Lockout.DefaultLockoutTimeSpan);
        Assert.True(options.Lockout.AllowedForNewUsers);
    }

    [Fact]
    public void ConfigureIdentityOptions_RequiresUniqueEmail()
    {
        var options = new IdentityOptions();

        IdentityServiceCollectionExtensions.ConfigureIdentityOptions(options);

        Assert.True(options.User.RequireUniqueEmail);
    }
}
