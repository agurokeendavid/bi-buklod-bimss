namespace Bimss.Domain.Membership;

// Immutable — recorded once by the validation/matching process
// (BIMSS-035/036) and never edited. A re-run of validation records new
// rows rather than mutating old ones, so the batch's validation history
// stays intact for reviewer audit, same reasoning as MemberDocument having
// no update method.
public sealed class ImportValidationError
{
    public ImportValidationError(
        Guid id,
        Guid importBatchId,
        Guid? memberImportStagingId,
        string? fieldName,
        ImportValidationSeverity severity,
        string message,
        DateTimeOffset detectedAtUtc)
    {
        if (importBatchId == Guid.Empty)
        {
            throw new ArgumentException("Import batch is required.", nameof(importBatchId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Id = id;
        ImportBatchId = importBatchId;
        MemberImportStagingId = memberImportStagingId;
        FieldName = fieldName;
        Severity = severity;
        Message = message;
        DetectedAtUtc = detectedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid ImportBatchId { get; private set; }

    public Guid? MemberImportStagingId { get; private set; }

    public string? FieldName { get; private set; }

    public ImportValidationSeverity Severity { get; private set; }

    public string Message { get; private set; } = string.Empty;

    public DateTimeOffset DetectedAtUtc { get; private set; }
}
