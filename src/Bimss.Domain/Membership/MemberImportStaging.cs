using Bimss.Domain.Exceptions;

namespace Bimss.Domain.Membership;

// One row per source spreadsheet row within an ImportBatch, holding raw
// values before they are validated, matched against existing members, and
// promoted into the real domain entities (docs/DOMAIN_WORKFLOWS.md's
// migration workflow). The entity enforces the pipeline's own invariants
// (can't promote an unvalidated or already-promoted row) but does not decide
// *how* a row is validated or matched — those algorithms belong to the
// services introduced in BIMSS-035/036/037.
public sealed class MemberImportStaging
{
    public MemberImportStaging(Guid id, Guid importBatchId, int rowNumber, MemberImportStagingFields fields)
    {
        if (importBatchId == Guid.Empty)
        {
            throw new ArgumentException("Import batch is required.", nameof(importBatchId));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(rowNumber, 1);
        ArgumentNullException.ThrowIfNull(fields);

        Id = id;
        ImportBatchId = importBatchId;
        RowNumber = rowNumber;

        SubmittedAtRaw = fields.SubmittedAtRaw;
        FormEmail = fields.FormEmail;
        SubmissionType = fields.SubmissionType;
        LastName = fields.LastName;
        FirstName = fields.FirstName;
        MiddleName = fields.MiddleName;
        Suffix = fields.Suffix;
        DateOfBirthRaw = fields.DateOfBirthRaw;
        PlaceOfBirth = fields.PlaceOfBirth;
        CivilStatus = fields.CivilStatus;
        SpouseFullName = fields.SpouseFullName;
        EmployeeNumber = fields.EmployeeNumber;
        PositionDesignation = fields.PositionDesignation;
        OfficeUnit = fields.OfficeUnit;
        PermanentAppointmentDateRaw = fields.PermanentAppointmentDateRaw;
        ProofOfEmploymentNote = fields.ProofOfEmploymentNote;
        HighestEducationalAttainment = fields.HighestEducationalAttainment;
        DegreeOrCourse = fields.DegreeOrCourse;
        EligibilityType = fields.EligibilityType;
        EligibilityDetails = fields.EligibilityDetails;
        PresentAddress = fields.PresentAddress;
        ProvincialAddress = fields.ProvincialAddress;
        Landline = fields.Landline;
        MobileNumber = fields.MobileNumber;
        Email = fields.Email;
        ChildrenRaw = fields.ChildrenRaw;
        FatherFullName = fields.FatherFullName;
        MotherMaidenName = fields.MotherMaidenName;
        ParentsPresentAddress = fields.ParentsPresentAddress;
        BeneficiariesRaw = fields.BeneficiariesRaw;
        JoiningReason = fields.JoiningReason;
        PrivacyConsentRaw = fields.PrivacyConsentRaw;

        ValidationStatus = ImportRowValidationStatus.NotValidated;
        MatchStatus = ImportRowMatchStatus.NotEvaluated;
    }

    // EF Core materialization constructor: binds exactly the mapped scalar
    // properties (the public constructor's `fields` parameter is a bundling
    // record, not itself mapped, so EF can't use that constructor).
    private MemberImportStaging(
        Guid id,
        Guid importBatchId,
        int rowNumber,
        string? submittedAtRaw,
        string? formEmail,
        string? submissionType,
        string? lastName,
        string? firstName,
        string? middleName,
        string? suffix,
        string? dateOfBirthRaw,
        string? placeOfBirth,
        string? civilStatus,
        string? spouseFullName,
        string? employeeNumber,
        string? positionDesignation,
        string? officeUnit,
        string? permanentAppointmentDateRaw,
        string? proofOfEmploymentNote,
        string? highestEducationalAttainment,
        string? degreeOrCourse,
        string? eligibilityType,
        string? eligibilityDetails,
        string? presentAddress,
        string? provincialAddress,
        string? landline,
        string? mobileNumber,
        string? email,
        string? childrenRaw,
        string? fatherFullName,
        string? motherMaidenName,
        string? parentsPresentAddress,
        string? beneficiariesRaw,
        string? joiningReason,
        string? privacyConsentRaw,
        ImportRowValidationStatus validationStatus,
        Guid? matchedMemberId,
        ImportRowMatchStatus matchStatus,
        Guid? promotedMemberId)
    {
        Id = id;
        ImportBatchId = importBatchId;
        RowNumber = rowNumber;
        SubmittedAtRaw = submittedAtRaw;
        FormEmail = formEmail;
        SubmissionType = submissionType;
        LastName = lastName;
        FirstName = firstName;
        MiddleName = middleName;
        Suffix = suffix;
        DateOfBirthRaw = dateOfBirthRaw;
        PlaceOfBirth = placeOfBirth;
        CivilStatus = civilStatus;
        SpouseFullName = spouseFullName;
        EmployeeNumber = employeeNumber;
        PositionDesignation = positionDesignation;
        OfficeUnit = officeUnit;
        PermanentAppointmentDateRaw = permanentAppointmentDateRaw;
        ProofOfEmploymentNote = proofOfEmploymentNote;
        HighestEducationalAttainment = highestEducationalAttainment;
        DegreeOrCourse = degreeOrCourse;
        EligibilityType = eligibilityType;
        EligibilityDetails = eligibilityDetails;
        PresentAddress = presentAddress;
        ProvincialAddress = provincialAddress;
        Landline = landline;
        MobileNumber = mobileNumber;
        Email = email;
        ChildrenRaw = childrenRaw;
        FatherFullName = fatherFullName;
        MotherMaidenName = motherMaidenName;
        ParentsPresentAddress = parentsPresentAddress;
        BeneficiariesRaw = beneficiariesRaw;
        JoiningReason = joiningReason;
        PrivacyConsentRaw = privacyConsentRaw;
        ValidationStatus = validationStatus;
        MatchedMemberId = matchedMemberId;
        MatchStatus = matchStatus;
        PromotedMemberId = promotedMemberId;
    }

    public Guid Id { get; private set; }

    public Guid ImportBatchId { get; private set; }

    public int RowNumber { get; private set; }

    public string? SubmittedAtRaw { get; private set; }

    public string? FormEmail { get; private set; }

    public string? SubmissionType { get; private set; }

    public string? LastName { get; private set; }

    public string? FirstName { get; private set; }

    public string? MiddleName { get; private set; }

    public string? Suffix { get; private set; }

    public string? DateOfBirthRaw { get; private set; }

    public string? PlaceOfBirth { get; private set; }

    public string? CivilStatus { get; private set; }

    public string? SpouseFullName { get; private set; }

    public string? EmployeeNumber { get; private set; }

    public string? PositionDesignation { get; private set; }

    public string? OfficeUnit { get; private set; }

    public string? PermanentAppointmentDateRaw { get; private set; }

    public string? ProofOfEmploymentNote { get; private set; }

    public string? HighestEducationalAttainment { get; private set; }

    public string? DegreeOrCourse { get; private set; }

    public string? EligibilityType { get; private set; }

    public string? EligibilityDetails { get; private set; }

    public string? PresentAddress { get; private set; }

    public string? ProvincialAddress { get; private set; }

    public string? Landline { get; private set; }

    public string? MobileNumber { get; private set; }

    public string? Email { get; private set; }

    public string? ChildrenRaw { get; private set; }

    public string? FatherFullName { get; private set; }

    public string? MotherMaidenName { get; private set; }

    public string? ParentsPresentAddress { get; private set; }

    public string? BeneficiariesRaw { get; private set; }

    public string? JoiningReason { get; private set; }

    public string? PrivacyConsentRaw { get; private set; }

    public ImportRowValidationStatus ValidationStatus { get; private set; }

    public Guid? MatchedMemberId { get; private set; }

    public ImportRowMatchStatus MatchStatus { get; private set; }

    public Guid? PromotedMemberId { get; private set; }

    public void RecordValidation(bool isValid)
    {
        if (PromotedMemberId is not null)
        {
            throw new ConflictException("Cannot revise validation for a staging row that has already been promoted.");
        }

        ValidationStatus = isValid ? ImportRowValidationStatus.Valid : ImportRowValidationStatus.Invalid;
    }

    public void RecordMatch(Guid? matchedMemberId, ImportRowMatchStatus matchStatus)
    {
        if (PromotedMemberId is not null)
        {
            throw new ConflictException("Cannot revise the matched member for a staging row that has already been promoted.");
        }

        if (matchStatus == ImportRowMatchStatus.NotEvaluated)
        {
            throw new ArgumentException("Match status must reflect an evaluated outcome.", nameof(matchStatus));
        }

        MatchedMemberId = matchedMemberId;
        MatchStatus = matchStatus;
    }

    public void MarkPromoted(Guid memberId)
    {
        if (ValidationStatus != ImportRowValidationStatus.Valid)
        {
            throw new ConflictException("Only a validated staging row can be promoted.");
        }

        if (PromotedMemberId is not null)
        {
            throw new ConflictException("This staging row has already been promoted.");
        }

        if (memberId == Guid.Empty)
        {
            throw new ArgumentException("Member is required.", nameof(memberId));
        }

        PromotedMemberId = memberId;
    }
}
