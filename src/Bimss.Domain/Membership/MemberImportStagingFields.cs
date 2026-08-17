namespace Bimss.Domain.Membership;

// Bundles the raw, unvalidated values captured from a single migration-source
// row (docs/DATA_DICTIONARY.md's Excel field mapping) so MemberImportStaging's
// constructor doesn't take 30+ positional parameters. Every value is optional
// at this stage — staging captures whatever the source row contained;
// validation happens later (BIMSS-035) and its outcome is recorded on
// MemberImportStaging itself, not here.
//
// ChildrenRaw and BeneficiariesRaw intentionally bundle each repeating group
// (children; beneficiaries 1-N) into a single raw value instead of numbered
// columns (BeneficiaryName1, BeneficiaryName2, ...) — docs/DATA_DICTIONARY.md
// explicitly calls that column-per-item pattern out as something to avoid,
// and the exact split/delimiter rule for each group is still an open
// question ("do not auto-parse until delimiter/format is agreed"), so the
// raw capture stays a single opaque value until BIMSS-034/036 defines it.
public sealed record MemberImportStagingFields
{
    public string? SubmittedAtRaw { get; init; }

    public string? FormEmail { get; init; }

    public string? SubmissionType { get; init; }

    public string? LastName { get; init; }

    public string? FirstName { get; init; }

    public string? MiddleName { get; init; }

    public string? Suffix { get; init; }

    public string? DateOfBirthRaw { get; init; }

    public string? PlaceOfBirth { get; init; }

    public string? CivilStatus { get; init; }

    public string? SpouseFullName { get; init; }

    public string? EmployeeNumber { get; init; }

    public string? PositionDesignation { get; init; }

    public string? OfficeUnit { get; init; }

    public string? PermanentAppointmentDateRaw { get; init; }

    public string? ProofOfEmploymentNote { get; init; }

    public string? HighestEducationalAttainment { get; init; }

    public string? DegreeOrCourse { get; init; }

    public string? EligibilityType { get; init; }

    public string? EligibilityDetails { get; init; }

    public string? PresentAddress { get; init; }

    public string? ProvincialAddress { get; init; }

    public string? Landline { get; init; }

    public string? MobileNumber { get; init; }

    public string? Email { get; init; }

    public string? ChildrenRaw { get; init; }

    public string? FatherFullName { get; init; }

    public string? MotherMaidenName { get; init; }

    public string? ParentsPresentAddress { get; init; }

    public string? BeneficiariesRaw { get; init; }

    public string? JoiningReason { get; init; }

    public string? PrivacyConsentRaw { get; init; }
}
