using Bimss.Domain.Exceptions;

namespace Bimss.Domain.Membership;

// One row per legacy-member-data import run (docs/DOMAIN_WORKFLOWS.md's
// "Existing member migration / update" workflow). Owns only the batch's own
// lifecycle state; it does not hold an in-memory collection of its
// MemberImportStaging rows (a batch can be thousands of rows) — those are
// queried by ImportBatchId through a query service instead, per AGENTS.md's
// "avoid N+1 / use projections for lists".
public sealed class ImportBatch
{
    public ImportBatch(Guid id, string fileName, Guid uploadedByUserId, DateTimeOffset uploadedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        if (uploadedByUserId == Guid.Empty)
        {
            throw new ArgumentException("Uploading user is required.", nameof(uploadedByUserId));
        }

        Id = id;
        FileName = fileName;
        UploadedByUserId = uploadedByUserId;
        UploadedAtUtc = uploadedAtUtc;
        Status = ImportBatchStatus.Created;
    }

    // EF Core materialization constructor: binds exactly the mapped scalar
    // properties, same reasoning as Member's private constructor.
    private ImportBatch(
        Guid id,
        string fileName,
        Guid uploadedByUserId,
        DateTimeOffset uploadedAtUtc,
        ImportBatchStatus status,
        int? rowCount,
        DateTimeOffset? stagedAtUtc,
        DateTimeOffset? validatedAtUtc,
        DateTimeOffset? promotedAtUtc,
        DateTimeOffset? cancelledAtUtc,
        string? remarks)
    {
        Id = id;
        FileName = fileName;
        UploadedByUserId = uploadedByUserId;
        UploadedAtUtc = uploadedAtUtc;
        Status = status;
        RowCount = rowCount;
        StagedAtUtc = stagedAtUtc;
        ValidatedAtUtc = validatedAtUtc;
        PromotedAtUtc = promotedAtUtc;
        CancelledAtUtc = cancelledAtUtc;
        Remarks = remarks;
    }

    public Guid Id { get; private set; }

    public string FileName { get; private set; } = string.Empty;

    public Guid UploadedByUserId { get; private set; }

    public DateTimeOffset UploadedAtUtc { get; private set; }

    public ImportBatchStatus Status { get; private set; }

    public int? RowCount { get; private set; }

    public DateTimeOffset? StagedAtUtc { get; private set; }

    public DateTimeOffset? ValidatedAtUtc { get; private set; }

    public DateTimeOffset? PromotedAtUtc { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public string? Remarks { get; private set; }

    public void MarkStaged(int rowCount, DateTimeOffset occurredAtUtc)
    {
        if (Status != ImportBatchStatus.Created)
        {
            throw new ConflictException($"Cannot stage an import batch with status '{Status}'.");
        }

        ArgumentOutOfRangeException.ThrowIfNegative(rowCount);

        RowCount = rowCount;
        StagedAtUtc = occurredAtUtc;
        Status = ImportBatchStatus.Staged;
    }

    public void MarkValidated(DateTimeOffset occurredAtUtc)
    {
        if (Status != ImportBatchStatus.Staged)
        {
            throw new ConflictException($"Cannot validate an import batch with status '{Status}'.");
        }

        ValidatedAtUtc = occurredAtUtc;
        Status = ImportBatchStatus.Validated;
    }

    public void MarkPromoted(DateTimeOffset occurredAtUtc)
    {
        if (Status != ImportBatchStatus.Validated)
        {
            throw new ConflictException($"Cannot promote an import batch with status '{Status}'.");
        }

        PromotedAtUtc = occurredAtUtc;
        Status = ImportBatchStatus.Promoted;
    }

    public void Cancel(DateTimeOffset occurredAtUtc, string? remarks)
    {
        if (Status is ImportBatchStatus.Promoted or ImportBatchStatus.Cancelled)
        {
            throw new ConflictException($"Cannot cancel an import batch with status '{Status}'.");
        }

        CancelledAtUtc = occurredAtUtc;
        Remarks = remarks;
        Status = ImportBatchStatus.Cancelled;
    }
}
