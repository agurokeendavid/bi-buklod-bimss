using System.Globalization;
using Bimss.Application.Auditing;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

// Implements docs/DOMAIN_WORKFLOWS.md's third and fourth migration steps:
// "Match possible existing member -> Detect duplicate Employee Number /
// identity candidates." Runs after validation (BIMSS-035) but does not
// require every row to be Valid — a row can be independently flagged as a
// duplicate regardless of unrelated field errors. Only records what it
// finds on each row (MemberImportStaging.RecordMatch); it does not decide
// whether a flagged row should still be promoted — that is the reviewer's
// call, surfaced by BIMSS-038's admin UI and enforced at BIMSS-037's
// promotion step.
public sealed class ImportBatchMatchingService(IImportBatchRepository importBatchRepository, IAuditLogger auditLogger)
{
    public async Task MatchAsync(Guid importBatchId, Guid? actorUserId, CancellationToken cancellationToken = default)
    {
        var batch = await importBatchRepository.GetTrackedByIdAsync(importBatchId, cancellationToken)
            ?? throw new NotFoundException("ImportBatch", importBatchId);

        if (batch.Status != ImportBatchStatus.Validated)
        {
            throw new ConflictException($"Cannot match an import batch with status '{batch.Status}'. It must be validated first.");
        }

        var rows = await importBatchRepository.GetTrackedRowsByBatchIdAsync(importBatchId, cancellationToken);

        foreach (var row in rows)
        {
            await MatchRowAsync(row, cancellationToken);
        }

        await importBatchRepository.SaveChangesAsync(cancellationToken);

        await auditLogger.LogAsync(
            new AuditEntry(actorUserId, "ImportBatch.Match", "ImportBatch", importBatchId.ToString(), AuditResult.Success),
            cancellationToken);
    }

    private async Task MatchRowAsync(MemberImportStaging row, CancellationToken cancellationToken)
    {
        // An exact BI employee number match is a confirmed duplicate — the
        // number is unique and mandatory (docs/DATA_DICTIONARY.md's
        // "Confirmed decisions"), so an existing member with the same
        // number is the same person, not a lookalike.
        if (!string.IsNullOrWhiteSpace(row.EmployeeNumber))
        {
            var confirmedMatch = await importBatchRepository.FindMemberIdByEmployeeNumberAsync(row.EmployeeNumber, cancellationToken);
            if (confirmedMatch is not null)
            {
                row.RecordMatch(confirmedMatch, ImportRowMatchStatus.ConfirmedDuplicate);
                return;
            }
        }

        // No confirmed match on employee number — check for a same
        // name + date-of-birth candidate. This is only a possible match
        // (same name/DOB does not prove the same person), left for a
        // reviewer to confirm or dismiss, not auto-resolved here.
        if (!string.IsNullOrWhiteSpace(row.LastName)
            && !string.IsNullOrWhiteSpace(row.FirstName)
            && TryParseDate(row.DateOfBirthRaw, out var dateOfBirth))
        {
            var possibleMatch = await importBatchRepository.FindMemberIdByNameAndDateOfBirthAsync(
                row.LastName, row.FirstName, dateOfBirth, cancellationToken);
            if (possibleMatch is not null)
            {
                row.RecordMatch(possibleMatch, ImportRowMatchStatus.PossibleDuplicate);
                return;
            }
        }

        row.RecordMatch(matchedMemberId: null, ImportRowMatchStatus.NoMatch);
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
