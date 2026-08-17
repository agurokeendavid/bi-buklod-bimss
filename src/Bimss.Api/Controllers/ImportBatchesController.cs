using System.Security.Claims;
using Bimss.Application.Membership;
using Bimss.Contracts.Membership;
using Bimss.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bimss.Api.Controllers;

// Existing-member migration (BIMSS-033–037). Gated the same as the rest of
// membership administration — no dedicated Import permission exists yet,
// and importing legacy records is a Membership Officer action per
// docs/design/BIMSS-UI-SPEC.md's roles table, same as creating/verifying
// members directly.
[ApiController]
[Route("api/import-batches")]
[Authorize(Policy = Permission.Membership.Manage)]
public class ImportBatchesController(
    IImportBatchQueryService importBatchQueryService,
    ImportBatchIngestionService importBatchIngestionService,
    ImportBatchValidationService importBatchValidationService,
    ImportBatchMatchingService importBatchMatchingService,
    ImportBatchPromotionService importBatchPromotionService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var batches = await importBatchQueryService.ListAsync(cancellationToken);

        var response = batches.Select(batch => new ImportBatchSummaryResponse
        {
            Id = batch.Id,
            FileName = batch.FileName,
            Status = batch.Status.ToString(),
            RowCount = batch.RowCount,
            UploadedAtUtc = batch.UploadedAtUtc,
            UploadedByUserId = batch.UploadedByUserId,
        });

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var batch = await importBatchQueryService.GetByIdAsync(id, cancellationToken);
        if (batch is null)
        {
            return NotFound();
        }

        return Ok(ToDetailResponse(batch));
    }

    [HttpGet("{id:guid}/rows")]
    public async Task<IActionResult> ListRows(Guid id, CancellationToken cancellationToken)
    {
        var rows = await importBatchQueryService.ListRowsByBatchIdAsync(id, cancellationToken);

        var response = rows.Select(row => new MemberImportStagingRowResponse
        {
            Id = row.Id,
            RowNumber = row.RowNumber,
            LastName = row.LastName,
            FirstName = row.FirstName,
            EmployeeNumber = row.EmployeeNumber,
            ValidationStatus = row.ValidationStatus.ToString(),
            MatchStatus = row.MatchStatus.ToString(),
            MatchedMemberId = row.MatchedMemberId,
            PromotedMemberId = row.PromotedMemberId,
        });

        return Ok(response);
    }

    [HttpGet("{id:guid}/errors")]
    public async Task<IActionResult> ListErrors(Guid id, CancellationToken cancellationToken)
    {
        var errors = await importBatchQueryService.ListErrorsByBatchIdAsync(id, cancellationToken);

        var response = errors.Select(error => new ImportValidationErrorResponse
        {
            Id = error.Id,
            MemberImportStagingId = error.MemberImportStagingId,
            FieldName = error.FieldName,
            Severity = error.Severity.ToString(),
            Message = error.Message,
        });

        return Ok(response);
    }

    [HttpPost]
    [RequestSizeLimit(10_485_760)] // 10 MB — same undocumented-elsewhere default as MemberDocumentsController.Upload.
    public async Task<IActionResult> Ingest([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("A file is required.");
        }

        var actorUserId = GetActorUserId();
        if (actorUserId is null)
        {
            return Unauthorized();
        }

        await using var stream = file.OpenReadStream();
        var result = await importBatchIngestionService.IngestAsync(file.FileName, stream, actorUserId.Value, cancellationToken);

        return CreatedAtAction(
            nameof(GetById), new { id = result.ImportBatchId }, new ImportBatchIngestResponse { Id = result.ImportBatchId, RowCount = result.RowCount });
    }

    [HttpPost("{id:guid}/validate")]
    public async Task<IActionResult> Validate(Guid id, CancellationToken cancellationToken)
    {
        await importBatchValidationService.ValidateAsync(id, GetActorUserId(), cancellationToken);

        var batch = await importBatchQueryService.GetByIdAsync(id, cancellationToken);
        return Ok(ToDetailResponse(batch!));
    }

    [HttpPost("{id:guid}/match")]
    public async Task<IActionResult> Match(Guid id, CancellationToken cancellationToken)
    {
        await importBatchMatchingService.MatchAsync(id, GetActorUserId(), cancellationToken);

        var batch = await importBatchQueryService.GetByIdAsync(id, cancellationToken);
        return Ok(ToDetailResponse(batch!));
    }

    [HttpPost("{id:guid}/rows/{rowId:guid}/promote")]
    public async Task<IActionResult> PromoteRow(Guid id, Guid rowId, CancellationToken cancellationToken)
    {
        var result = await importBatchPromotionService.PromoteRowAsync(rowId, GetActorUserId(), cancellationToken);

        return Ok(new PromoteImportRowResponse { MemberId = result.MemberId });
    }

    private Guid? GetActorUserId()
    {
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedActorUserId)
            ? parsedActorUserId
            : null;
    }

    private static ImportBatchDetailResponse ToDetailResponse(ImportBatchDetail batch) => new()
    {
        Id = batch.Id,
        FileName = batch.FileName,
        Status = batch.Status.ToString(),
        RowCount = batch.RowCount,
        UploadedAtUtc = batch.UploadedAtUtc,
        UploadedByUserId = batch.UploadedByUserId,
        StagedAtUtc = batch.StagedAtUtc,
        ValidatedAtUtc = batch.ValidatedAtUtc,
        PromotedAtUtc = batch.PromotedAtUtc,
        CancelledAtUtc = batch.CancelledAtUtc,
        Remarks = batch.Remarks,
    };
}
