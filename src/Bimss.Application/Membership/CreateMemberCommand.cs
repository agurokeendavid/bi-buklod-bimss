namespace Bimss.Application.Membership;

// Deliberately scoped to Member's own required fields plus employment,
// since BI Employee Number is mandatory at creation (confirmed with
// Buklod, 2026-08-14 — docs/DATA_DICTIONARY.md). Contact, address,
// education, eligibility, family, children, privacy consent, and documents
// are added afterward through their own operations (the officer-review
// edit workflow, Phase 1E), not bundled into this command.
public sealed record CreateMemberCommand(
    string LastName,
    string FirstName,
    string? MiddleName,
    Guid? SuffixId,
    DateOnly DateOfBirth,
    string PlaceOfBirth,
    Guid CivilStatusId,
    string? JoiningReason,
    string EmployeeNumber,
    string PositionDesignation,
    Guid OfficeUnitId,
    DateOnly? PermanentAppointmentDate);
