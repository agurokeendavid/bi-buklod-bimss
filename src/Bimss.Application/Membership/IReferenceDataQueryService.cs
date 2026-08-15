namespace Bimss.Application.Membership;

// Scoped to exactly what BIMSS-029's create-member form needs
// (CivilStatus, Suffix, OfficeUnit) — the other reference tables
// (EducationalAttainment, EligibilityType, RelationshipType,
// MemberStatusReason) get their own query methods added when a task
// actually needs them, not speculatively here.
public interface IReferenceDataQueryService
{
    Task<IReadOnlyList<ReferenceDataSummary>> ListCivilStatusesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<ReferenceDataSummary>> ListSuffixesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<ReferenceDataSummary>> ListOfficeUnitsAsync(CancellationToken cancellationToken);
}
