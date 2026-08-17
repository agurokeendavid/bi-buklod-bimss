using System.Globalization;
using Bimss.Application.Auditing;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

// Implements docs/DOMAIN_WORKFLOWS.md's second migration step: "Validate
// required/format fields." Checks only what's already a confirmed business
// rule — the same required fields Member/MemberEmployment's own
// constructors enforce (docs/DATA_DICTIONARY.md's "Confirmed decisions"),
// plus whether the raw CivilStatus/OfficeUnit/Suffix text resolves to a
// known reference-data value, since IReferenceDataQueryService already
// exists for exactly that lookup. Does not match against existing members
// or flag duplicates — that is BIMSS-036. Does not require an exact date
// format from Buklod; DateOnly.TryParse's culture-aware parsing is used
// rather than guessing a single hard-coded format.
public sealed class ImportBatchValidationService(
    IImportBatchRepository importBatchRepository,
    IReferenceDataQueryService referenceDataQueryService,
    IAuditLogger auditLogger,
    TimeProvider timeProvider)
{
    public async Task ValidateAsync(Guid importBatchId, Guid? actorUserId, CancellationToken cancellationToken = default)
    {
        var batch = await importBatchRepository.GetTrackedByIdAsync(importBatchId, cancellationToken)
            ?? throw new NotFoundException("ImportBatch", importBatchId);

        var rows = await importBatchRepository.GetTrackedRowsByBatchIdAsync(importBatchId, cancellationToken);

        var civilStatusNames = await LoadReferenceNamesAsync(referenceDataQueryService.ListCivilStatusesAsync, cancellationToken);
        var officeUnitNames = await LoadReferenceNamesAsync(referenceDataQueryService.ListOfficeUnitsAsync, cancellationToken);
        var suffixNames = await LoadReferenceNamesAsync(referenceDataQueryService.ListSuffixesAsync, cancellationToken);

        var occurredAtUtc = timeProvider.GetUtcNow();
        var errors = new List<ImportValidationError>();

        foreach (var row in rows)
        {
            var rowIssues = ValidateRow(row, civilStatusNames, officeUnitNames, suffixNames);
            errors.AddRange(rowIssues.Select(issue => new ImportValidationError(
                Guid.NewGuid(), importBatchId, row.Id, issue.FieldName, issue.Severity, issue.Message, occurredAtUtc)));

            row.RecordValidation(isValid: rowIssues.TrueForAll(issue => issue.Severity != ImportValidationSeverity.Error));
        }

        if (errors.Count > 0)
        {
            await importBatchRepository.AddValidationErrorsAsync(errors, cancellationToken);
        }

        batch.MarkValidated(occurredAtUtc);

        await importBatchRepository.SaveChangesAsync(cancellationToken);

        await auditLogger.LogAsync(
            new AuditEntry(actorUserId, "ImportBatch.Validate", "ImportBatch", importBatchId.ToString(), AuditResult.Success),
            cancellationToken);
    }

    private static List<RowIssue> ValidateRow(
        MemberImportStaging row,
        IReadOnlySet<string> civilStatusNames,
        IReadOnlySet<string> officeUnitNames,
        IReadOnlySet<string> suffixNames)
    {
        var issues = new List<RowIssue>();

        RequireNonBlank(issues, nameof(MemberImportStaging.LastName), "Last name is required.", row.LastName);
        RequireNonBlank(issues, nameof(MemberImportStaging.FirstName), "First name is required.", row.FirstName);
        RequireNonBlank(issues, nameof(MemberImportStaging.PlaceOfBirth), "Place of birth is required.", row.PlaceOfBirth);
        RequireNonBlank(issues, nameof(MemberImportStaging.EmployeeNumber), "BI employee number is required.", row.EmployeeNumber);
        RequireNonBlank(
            issues, nameof(MemberImportStaging.PositionDesignation), "Position/designation is required.", row.PositionDesignation);

        ValidateDate(issues, nameof(MemberImportStaging.DateOfBirthRaw), "Date of birth", row.DateOfBirthRaw, required: true);
        ValidateDate(
            issues,
            nameof(MemberImportStaging.PermanentAppointmentDateRaw),
            "Date of permanent appointment",
            row.PermanentAppointmentDateRaw,
            required: false);

        ValidateReference(
            issues,
            nameof(MemberImportStaging.CivilStatus),
            "Civil status",
            row.CivilStatus,
            civilStatusNames,
            required: true,
            severity: ImportValidationSeverity.Error);
        ValidateReference(
            issues,
            nameof(MemberImportStaging.OfficeUnit),
            "Office unit",
            row.OfficeUnit,
            officeUnitNames,
            required: true,
            severity: ImportValidationSeverity.Error);
        ValidateReference(
            issues,
            nameof(MemberImportStaging.Suffix),
            "Suffix",
            row.Suffix,
            suffixNames,
            required: false,
            severity: ImportValidationSeverity.Warning);

        return issues;
    }

    private static void RequireNonBlank(List<RowIssue> issues, string fieldName, string message, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            issues.Add(new RowIssue(fieldName, ImportValidationSeverity.Error, message));
        }
    }

    private static void ValidateDate(List<RowIssue> issues, string fieldName, string label, string? raw, bool required)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            if (required)
            {
                issues.Add(new RowIssue(fieldName, ImportValidationSeverity.Error, $"{label} is required."));
            }

            return;
        }

        if (!DateOnly.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
            && !DateOnly.TryParse(raw, CultureInfo.CurrentCulture, DateTimeStyles.None, out _))
        {
            issues.Add(new RowIssue(fieldName, ImportValidationSeverity.Error, $"{label} '{raw}' is not a recognizable date."));
        }
    }

    private static void ValidateReference(
        List<RowIssue> issues,
        string fieldName,
        string label,
        string? raw,
        IReadOnlySet<string> knownNames,
        bool required,
        ImportValidationSeverity severity)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            if (required)
            {
                issues.Add(new RowIssue(fieldName, ImportValidationSeverity.Error, $"{label} is required."));
            }

            return;
        }

        if (!knownNames.Contains(raw.Trim()))
        {
            issues.Add(new RowIssue(fieldName, severity, $"{label} '{raw}' does not match a known reference value."));
        }
    }

    private static async Task<IReadOnlySet<string>> LoadReferenceNamesAsync(
        Func<CancellationToken, Task<IReadOnlyList<ReferenceDataSummary>>> list, CancellationToken cancellationToken)
    {
        var items = await list(cancellationToken);
        return items.Select(item => item.Name.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private readonly record struct RowIssue(string? FieldName, ImportValidationSeverity Severity, string Message);
}
