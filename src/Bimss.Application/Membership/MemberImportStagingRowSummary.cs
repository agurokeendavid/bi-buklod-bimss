using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

// Deliberately narrow — the officer reviewing staged rows needs just enough
// to identify a row and its pipeline status, not every raw captured field.
// (Full raw values are available via the row's ImportValidationErrors if a
// field is in question.)
public sealed record MemberImportStagingRowSummary(
    Guid Id,
    int RowNumber,
    string? LastName,
    string? FirstName,
    string? EmployeeNumber,
    ImportRowValidationStatus ValidationStatus,
    ImportRowMatchStatus MatchStatus,
    Guid? MatchedMemberId,
    Guid? PromotedMemberId);
