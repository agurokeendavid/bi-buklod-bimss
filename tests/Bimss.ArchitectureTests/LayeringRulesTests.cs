using Bimss.Application.Auditing;
using Bimss.Domain.Authorization;
using NetArchTest.Rules;

namespace Bimss.ArchitectureTests;

public class LayeringRulesTests
{
    [Fact]
    public void Domain_DoesNotDependOn_EntityFrameworkCoreOrAspNetCore()
    {
        var result = Types.InAssembly(typeof(Permission).Assembly)
            .Should()
            .NotHaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, DescribeFailures(result));
    }

    [Fact]
    public void Application_DoesNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(typeof(IAuditLogger).Assembly)
            .Should()
            .NotHaveDependencyOn("Bimss.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, DescribeFailures(result));
    }

    private static string DescribeFailures(TestResult result) =>
        result.FailingTypes is null
            ? string.Empty
            : "Violating types: " + string.Join(", ", result.FailingTypes.Select(type => type.FullName));
}
