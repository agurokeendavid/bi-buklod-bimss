using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

// Self-service projection — reference values are already resolved to
// display names (unlike MemberDetail, which keeps raw ids for the
// officer-facing edit form's Select components).
public sealed record MyProfileDetail(
    Guid Id,
    string LastName,
    string FirstName,
    string? MiddleName,
    string? SuffixName,
    DateOnly DateOfBirth,
    string PlaceOfBirth,
    string CivilStatusName,
    string? JoiningReason,
    MemberStatus Status,
    string EmployeeNumber,
    string PositionDesignation,
    string OfficeUnitName,
    DateOnly? PermanentAppointmentDate);
