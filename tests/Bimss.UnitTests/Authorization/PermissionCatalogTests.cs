using Bimss.Domain.Authorization;

namespace Bimss.UnitTests.Authorization;

public class PermissionCatalogTests
{
    private static readonly string[] ExpectedPermissions =
    [
        "Membership.ViewSelf",
        "Membership.ManageSelf",
        "Membership.Manage",
        "Membership.Verify",
        "Beneficiary.ManageSelf",
        "Beneficiary.Approve",
        "Contribution.ViewSelf",
        "Contribution.Manage",
        "Loan.Apply",
        "Loan.ViewSelf",
        "Loan.Review",
        "Loan.Approve",
        "Loan.Release",
        "Election.Vote",
        "Election.Manage",
        "Election.Finalize",
        "Report.ViewMembership",
        "Report.ViewFinance",
        "Audit.View",
    ];

    [Fact]
    public void All_MatchesArchitectureDocPermissionCatalog()
    {
        Assert.Equal(ExpectedPermissions.OrderBy(name => name), Permission.All.OrderBy(name => name));
    }

    [Fact]
    public void All_ContainsNoDuplicates()
    {
        Assert.Equal(Permission.All.Count, Permission.All.Distinct().Count());
    }
}
