using System.Security.Claims;
using Bimss.Application.Membership;
using Bimss.Contracts.Membership;
using Bimss.Domain.Authorization;
using Bimss.Domain.Membership;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bimss.Api.Controllers;

// Officer-facing review queue for docs/DOMAIN_WORKFLOWS.md's "Member
// profile update" workflow. Flat route (not nested under /members/{id}) —
// this is a cross-member queue an officer works through, same shape as the
// Approvals screen in docs/design/BIMSS-UI-SPEC.md.
[ApiController]
[Route("api/update-requests")]
[Authorize(Policy = Permission.Membership.Manage)]
public class MemberUpdateRequestsController(
    IMemberUpdateRequestQueryService queryService, MemberUpdateRequestReviewService reviewService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status, CancellationToken cancellationToken)
    {
        MemberUpdateRequestStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<MemberUpdateRequestStatus>(status, ignoreCase: true, out var value))
            {
                return BadRequest($"Unrecognized status '{status}'.");
            }

            parsedStatus = value;
        }

        var requests = await queryService.ListAsync(parsedStatus, cancellationToken);

        var response = requests.Select(request => new MemberUpdateRequestSummaryResponse
        {
            Id = request.Id,
            MemberId = request.MemberId,
            MemberLastName = request.MemberLastName,
            MemberFirstName = request.MemberFirstName,
            SubmittedByUserId = request.SubmittedByUserId,
            SubmittedAtUtc = request.SubmittedAtUtc,
            Status = request.Status.ToString(),
        });

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var request = await queryService.GetByIdAsync(id, cancellationToken);
        if (request is null)
        {
            return NotFound();
        }

        return Ok(ToDetailResponse(request));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ReviewMemberUpdateRequestRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        if (actorUserId is null)
        {
            return Unauthorized();
        }

        await reviewService.ApproveAsync(id, actorUserId.Value, request.Remarks, cancellationToken);

        var updated = await queryService.GetByIdAsync(id, cancellationToken);
        return Ok(ToDetailResponse(updated!));
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ReviewMemberUpdateRequestRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Remarks))
        {
            return BadRequest("Remarks are required to reject an update request.");
        }

        var actorUserId = GetActorUserId();
        if (actorUserId is null)
        {
            return Unauthorized();
        }

        await reviewService.RejectAsync(id, actorUserId.Value, request.Remarks, cancellationToken);

        var updated = await queryService.GetByIdAsync(id, cancellationToken);
        return Ok(ToDetailResponse(updated!));
    }

    private Guid? GetActorUserId()
    {
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedActorUserId)
            ? parsedActorUserId
            : null;
    }

    private static MemberUpdateRequestDetailResponse ToDetailResponse(MemberUpdateRequestDetail request) => new()
    {
        Id = request.Id,
        MemberId = request.MemberId,
        MemberLastName = request.MemberLastName,
        MemberFirstName = request.MemberFirstName,
        SubmittedByUserId = request.SubmittedByUserId,
        SubmittedAtUtc = request.SubmittedAtUtc,
        Status = request.Status.ToString(),
        ReviewedByUserId = request.ReviewedByUserId,
        ReviewedAtUtc = request.ReviewedAtUtc,
        ReviewRemarks = request.ReviewRemarks,
        Changes = [.. request.Changes.Select(change => new MemberUpdateRequestChangeResponse
        {
            Id = change.Id,
            FieldName = change.FieldName,
            OldValue = change.OldValue,
            NewValue = change.NewValue,
        })],
    };
}
