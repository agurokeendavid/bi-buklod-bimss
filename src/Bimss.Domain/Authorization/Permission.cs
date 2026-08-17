namespace Bimss.Domain.Authorization;

public static class Permission
{
    public const string ClaimType = "bimss:permission";

    public static class Membership
    {
        public const string ViewSelf = "Membership.ViewSelf";

        // Submitting/tracking one's own update requests (BIMSS-042/045) and
        // direct self-service edit of low-risk fields (BIMSS-044) — distinct
        // from ViewSelf (read-only) and Manage (officer, any member).
        public const string ManageSelf = "Membership.ManageSelf";
        public const string Manage = "Membership.Manage";
        public const string Verify = "Membership.Verify";
    }

    public static class Beneficiary
    {
        public const string ManageSelf = "Beneficiary.ManageSelf";
        public const string Approve = "Beneficiary.Approve";
    }

    public static class Contribution
    {
        public const string ViewSelf = "Contribution.ViewSelf";
        public const string Manage = "Contribution.Manage";
    }

    public static class Loan
    {
        public const string Apply = "Loan.Apply";
        public const string ViewSelf = "Loan.ViewSelf";
        public const string Review = "Loan.Review";
        public const string Approve = "Loan.Approve";
        public const string Release = "Loan.Release";
    }

    public static class Election
    {
        public const string Vote = "Election.Vote";
        public const string Manage = "Election.Manage";
        public const string Finalize = "Election.Finalize";
    }

    public static class Report
    {
        public const string ViewMembership = "Report.ViewMembership";
        public const string ViewFinance = "Report.ViewFinance";
    }

    public static class Audit
    {
        public const string View = "Audit.View";
    }

    public static IReadOnlyCollection<string> All { get; } =
    [
        Membership.ViewSelf,
        Membership.ManageSelf,
        Membership.Manage,
        Membership.Verify,
        Beneficiary.ManageSelf,
        Beneficiary.Approve,
        Contribution.ViewSelf,
        Contribution.Manage,
        Loan.Apply,
        Loan.ViewSelf,
        Loan.Review,
        Loan.Approve,
        Loan.Release,
        Election.Vote,
        Election.Manage,
        Election.Finalize,
        Report.ViewMembership,
        Report.ViewFinance,
        Audit.View,
    ];
}
