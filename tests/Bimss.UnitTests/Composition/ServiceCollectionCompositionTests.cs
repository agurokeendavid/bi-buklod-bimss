using Bimss.Application;
using Bimss.Application.Auditing;
using Bimss.Application.Membership;
using Bimss.Infrastructure;
using Bimss.Infrastructure.Authorization;
using Bimss.Infrastructure.Identity;
using Bimss.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bimss.UnitTests.Composition;

public class ServiceCollectionCompositionTests
{
    [Fact]
    public async Task AddBimssInfrastructureApplicationAndAuthorization_ResolveTheirCoreServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Bimss"] = "Server=(localdb)\\mssqllocaldb;Database=Composition;Trusted_Connection=True;",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddBimssInfrastructure(configuration);
        services.AddBimssApplication();
        services.AddBimssAuthorization();

        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<BimssDbContext>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IAuditLogger>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IMemberDocumentStorage>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IMemberRepository>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IMemberQueryService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IReferenceDataQueryService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<MemberCreationService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<MemberStatusTransitionService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<MemberProfileUpdateService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IMemberDocumentQueryService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<MemberDocumentUploadService>());
        Assert.NotNull(provider.GetRequiredService<IAuthorizationPolicyProvider>());
    }
}
