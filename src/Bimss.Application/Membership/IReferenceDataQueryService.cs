namespace Bimss.Application.Membership;

// Scoped to exactly what has needed it so far: BIMSS-029's create-member
// form (CivilStatus, Suffix, OfficeUnit) and BIMSS-031's deactivate-member
// dialog (MemberStatusReason). The remaining reference tables
// (EducationalAttainment, EligibilityType, RelationshipType) get their own
// query methods added when a task actually needs them, not speculatively
// here.
public interface IReferenceDataQueryService
{
    Task<IReadOnlyList<ReferenceDataSummary>> ListCivilStatusesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<ReferenceDataSummary>> ListSuffixesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<ReferenceDataSummary>> ListOfficeUnitsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<ReferenceDataSummary>> ListMemberStatusReasonsAsync(CancellationToken cancellationToken);
}
