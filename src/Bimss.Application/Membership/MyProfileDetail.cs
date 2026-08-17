using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

// Self-service projection — reference values carry both the display name
// (used by the read-only "My Profile" view) and the raw id (needed by
// BIMSS-042's edit form to pre-select the right option in each Select,
// same as MemberDetail does for the officer-facing edit form).
public sealed record MyProfileDetail(
    Guid Id,
    string LastName,
    string FirstName,
    string? MiddleName,
    Guid? SuffixId,
    string? SuffixName,
    DateOnly DateOfBirth,
    string PlaceOfBirth,
    Guid CivilStatusId,
    string CivilStatusName,
    string? JoiningReason,
    MemberStatus Status,
    string EmployeeNumber,
    string PositionDesignation,
    Guid OfficeUnitId,
    string OfficeUnitName,
    DateOnly? PermanentAppointmentDate);
