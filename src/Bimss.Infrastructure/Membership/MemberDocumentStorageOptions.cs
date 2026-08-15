namespace Bimss.Infrastructure.Membership;

public sealed class MemberDocumentStorageOptions
{
    public const string SectionName = "DocumentStorage";

    // Outside wwwroot/executable locations by default, per
    // docs/SECURITY_AND_PRIVACY.md's "store outside executable/static web
    // locations" rule.
    public string RootPath { get; set; } = "App_Data/MemberDocuments";
}
