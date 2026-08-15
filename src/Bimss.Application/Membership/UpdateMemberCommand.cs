namespace Bimss.Application.Membership;

// Mirrors CreateMemberCommand minus EmployeeNumber, which is a business
// identifier (AGENTS.md) and not editable through this command — see
// MemberEmployment, which never exposes a way to change it after creation.
public sealed record UpdateMemberCommand(
    string LastName,
    string FirstName,
    string? MiddleName,
    Guid? SuffixId,
    DateOnly DateOfBirth,
    string PlaceOfBirth,
    Guid CivilStatusId,
    string? JoiningReason,
    string PositionDesignation,
    Guid OfficeUnitId,
    DateOnly? PermanentAppointmentDate);
