using System.Globalization;
using Bimss.Application.Auditing;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

// Implements docs/DOMAIN_WORKFLOWS.md's final migration steps: "Reviewer
// confirms -> Create/update member through normal application services ->
// Record migration audit." Promotes exactly one staging row at a time —
// the workflow expects a reviewer to look at (especially flagged) rows
// individually, not a blind bulk commit of the whole batch. Deliberately
// scoped to Member + MemberEmployment only, the same fields
// CreateMemberCommand already supports — MemberContact/MemberAddress/
// MemberEducation/MemberEligibility/MemberFamilyInformation/
// MemberPrivacyConsent have no Application-layer creation capability
// anywhere in the codebase yet (CreateMemberCommand's own comment: "added
// afterward through their own operations, the officer-review edit
// workflow, Phase 1E, not bundled into this command") — building that
// capability speculatively here, with no other caller, would be exactly
// the kind of ahead-of-need work AGENTS.md warns against. MemberChild and
// MemberBeneficiary stay unpromoted for the same reason BIMSS-034 left
// ChildrenRaw/BeneficiariesRaw unparsed: no agreed splitting rule yet.
public sealed class ImportBatchPromotionService(
    IImportBatchRepository importBatchRepository,
    IMemberRepository memberRepository,
    IReferenceDataQueryService referenceDataQueryService,
    IAuditLogger auditLogger,
    TimeProvider timeProvider)
{
    public async Task<ImportBatchPromotionResult> PromoteRowAsync(
        Guid stagingRowId, Guid? actorUserId, CancellationToken cancellationToken = default)
    {
        var row = await importBatchRepository.GetTrackedRowByIdAsync(stagingRowId, cancellationToken)
            ?? throw new NotFoundException("MemberImportStaging", stagingRowId);

        if (row.ValidationStatus != ImportRowValidationStatus.Valid)
        {
            throw new ConflictException("Cannot promote a staging row that has not passed validation.");
        }

        if (row.MatchStatus == ImportRowMatchStatus.NotEvaluated)
        {
            throw new ConflictException("Cannot promote a staging row that has not been matched against existing members yet.");
        }

        if (row.MatchStatus != ImportRowMatchStatus.NoMatch)
        {
            throw new ConflictException(
                "This row is flagged as a possible or confirmed duplicate and requires manual review before it can be promoted.");
        }

        if (await memberRepository.EmployeeNumberExistsAsync(row.EmployeeNumber!, cancellationToken))
        {
            throw new ConflictException($"Employee number '{row.EmployeeNumber}' is already registered.");
        }

        var civilStatusId = await ResolveReferenceIdAsync(
            referenceDataQueryService.ListCivilStatusesAsync, row.CivilStatus, cancellationToken)
            ?? throw new DomainValidationException(
                "Civil status could not be resolved.",
                new Dictionary<string, string[]> { [nameof(MemberImportStaging.CivilStatus)] = ["Civil status does not match a known reference value."] });

        var officeUnitId = await ResolveReferenceIdAsync(
            referenceDataQueryService.ListOfficeUnitsAsync, row.OfficeUnit, cancellationToken)
            ?? throw new DomainValidationException(
                "Office unit could not be resolved.",
                new Dictionary<string, string[]> { [nameof(MemberImportStaging.OfficeUnit)] = ["Office unit does not match a known reference value."] });

        // Suffix is optional — an unresolvable value degrades to null
        // rather than blocking promotion, same as BIMSS-035's Warning-only
        // treatment of an unmatched Suffix.
        var suffixId = await ResolveReferenceIdAsync(referenceDataQueryService.ListSuffixesAsync, row.Suffix, cancellationToken);

        if (!TryParseDate(row.DateOfBirthRaw, out var dateOfBirth))
        {
            throw new DomainValidationException(
                "Date of birth could not be parsed.",
                new Dictionary<string, string[]> { [nameof(MemberImportStaging.DateOfBirthRaw)] = ["Date of birth is not a recognizable date."] });
        }

        DateOnly? permanentAppointmentDate = null;
        if (!string.IsNullOrWhiteSpace(row.PermanentAppointmentDateRaw))
        {
            if (!TryParseDate(row.PermanentAppointmentDateRaw, out var parsedAppointmentDate))
            {
                throw new DomainValidationException(
                    "Date of permanent appointment could not be parsed.",
                    new Dictionary<string, string[]>
                    {
                        [nameof(MemberImportStaging.PermanentAppointmentDateRaw)] = ["Date of permanent appointment is not a recognizable date."],
                    });
            }

            permanentAppointmentDate = parsedAppointmentDate;
        }

        var occurredAtUtc = timeProvider.GetUtcNow();
        var memberId = Guid.NewGuid();

        var member = new Member(
            memberId,
            row.LastName!,
            row.FirstName!,
            row.MiddleName,
            suffixId,
            dateOfBirth,
            row.PlaceOfBirth!,
            civilStatusId,
            row.JoiningReason,
            occurredAtUtc);

        var employment = new MemberEmployment(
            Guid.NewGuid(), memberId, row.EmployeeNumber!, row.PositionDesignation!, officeUnitId, permanentAppointmentDate);

        row.MarkPromoted(memberId);

        await importBatchRepository.PromoteRowAsync(row, member, employment, cancellationToken);

        await auditLogger.LogAsync(
            new AuditEntry(
                actorUserId,
                "ImportBatch.PromoteRow",
                "Member",
                memberId.ToString(),
                AuditResult.Success,
                remarks: null,
                metadata: new Dictionary<string, string> { ["ImportBatchId"] = row.ImportBatchId.ToString(), ["StagingRowId"] = row.Id.ToString() }),
            cancellationToken);

        return new ImportBatchPromotionResult(memberId);
    }

    private static async Task<Guid?> ResolveReferenceIdAsync(
        Func<CancellationToken, Task<IReadOnlyList<ReferenceDataSummary>>> list, string? raw, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var items = await list(cancellationToken);
        var match = items.FirstOrDefault(item => string.Equals(item.Name.Trim(), raw.Trim(), StringComparison.OrdinalIgnoreCase));
        return match?.Id;
    }

    private static bool TryParseDate(string? raw, out DateOnly value)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            value = default;
            return false;
        }

        return DateOnly.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out value)
            || DateOnly.TryParse(raw, CultureInfo.CurrentCulture, DateTimeStyles.None, out value);
    }
}
