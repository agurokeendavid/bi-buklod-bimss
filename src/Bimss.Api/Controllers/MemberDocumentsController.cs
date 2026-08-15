using System.Security.Claims;
using Bimss.Application.Membership;
using Bimss.Contracts.Membership;
using Bimss.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bimss.Api.Controllers;

[ApiController]
[Route("api/members/{memberId:guid}/documents")]
[Authorize(Policy = Permission.Membership.Manage)]
public class MemberDocumentsController(
    IMemberDocumentQueryService memberDocumentQueryService,
    MemberDocumentUploadService memberDocumentUploadService,
    IMemberDocumentStorage memberDocumentStorage)
    : ControllerBase
{
    // Content-type allowlist is enforced again inside MemberDocument's
    // constructor (Domain re-validates regardless of what this boundary
    // already checked, per AGENTS.md); the extension check has no Domain
    // equivalent since MemberDocument never sees the original file name's
    // extension as a validated concept, so it belongs here.
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".jpg", ".jpeg", ".png" };

    [HttpGet]
    public async Task<IActionResult> List(Guid memberId, CancellationToken cancellationToken)
    {
        var documents = await memberDocumentQueryService.ListByMemberIdAsync(memberId, cancellationToken);

        var response = documents.Select(document => new MemberDocumentSummaryResponse
        {
            Id = document.Id,
            DocumentType = document.DocumentType,
            OriginalFileName = document.OriginalFileName,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            UploadedAtUtc = document.UploadedAtUtc,
            UploadedByUserId = document.UploadedByUserId,
        });

        return Ok(response);
    }

    [HttpPost]
    [RequestSizeLimit(10_485_760)] // 10 MB — no size limit is documented elsewhere in this repo; see BIMSS-032's PR notes.
    public async Task<IActionResult> Upload(
        Guid memberId, [FromForm] IFormFile file, [FromForm] string documentType, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("A file is required.");
        }

        if (string.IsNullOrWhiteSpace(documentType))
        {
            return BadRequest("Document type is required.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            return BadRequest($"File extension '{extension}' is not accepted. Allowed: PDF, JPG, PNG.");
        }

        var actorUserId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedActorUserId)
            ? parsedActorUserId
            : (Guid?)null;

        await using var stream = file.OpenReadStream();
        var documentId = await memberDocumentUploadService.UploadAsync(
            memberId, documentType, file.FileName, file.ContentType, stream, file.Length, actorUserId, cancellationToken);

        return CreatedAtAction(nameof(List), new { memberId }, new { id = documentId });
    }

    [HttpGet("{documentId:guid}/download")]
    public async Task<IActionResult> Download(Guid memberId, Guid documentId, CancellationToken cancellationToken)
    {
        var document = await memberDocumentQueryService.GetForDownloadAsync(memberId, documentId, cancellationToken);
        if (document is null)
        {
            return NotFound();
        }

        var content = await memberDocumentStorage.OpenReadAsync(document.StorageKey, cancellationToken);

        return File(content, document.ContentType, document.OriginalFileName);
    }
}
