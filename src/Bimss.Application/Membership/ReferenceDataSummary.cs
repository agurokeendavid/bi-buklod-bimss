namespace Bimss.Application.Membership;

// Shared projection for the reference/master data tables (CivilStatus,
// Suffix, OfficeUnit, ...) — they all share the same Id/Code/Name shape.
public sealed record ReferenceDataSummary(Guid Id, string Code, string Name);
