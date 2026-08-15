using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

// Projection for a single member's detail view. Employment fields are
// nullable because the join is optional at the query level — in practice
// every member created via MemberCreationService has exactly one employment
// record, but the projection stays robust rather than assuming that always
// holds.
public sealed record MemberDetail(
    Guid Id,
    string LastName,
    string FirstName,
    string? MiddleName,
    Guid? SuffixId,
    DateOnly DateOfBirth,
    string PlaceOfBirth,
    Guid CivilStatusId,
    string? JoiningReason,
    MemberStatus Status,
    string? EmployeeNumber,
    string? PositionDesignation,
    Guid? OfficeUnitId,
    DateOnly? PermanentAppointmentDate);
