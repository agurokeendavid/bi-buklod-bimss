using System.Security.Claims;
using Bimss.Application.Membership;
using Bimss.Contracts.Membership;
using Bimss.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bimss.Api.Controllers;

// Member self-service (Phase 1E) — submitting a profile-change request for
// officer review, and (BIMSS-045) checking its status/history afterward
// (docs/DOMAIN_WORKFLOWS.md's "Member profile update" workflow). Resolves
// "which member" from the caller's own user id on every action, same as
// MyProfileController — no way to submit or view a request on another
// member's behalf through this controller. Authorization is per-action
// (not class-level) so the read endpoints can use ViewSelf and the submit
// endpoint can use ManageSelf without stacking [Authorize] attributes,
// which AND-combine rather than OR (see AuthorizationPolicies.ReferenceDataRead's
// own note on this from BIMSS-042) — same shape as MyContactController.
[ApiController]
[Route("api/my/update-requests")]
public class MyUpdateRequestsController(
    IMemberQueryService memberQueryService,
    IMemberUpdateRequestQueryService updateRequestQueryService,
    MemberUpdateRequestSubmissionService submissionService)
    : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = Permission.Membership.ManageSelf)]
    public async Task<IActionResult> Submit([FromBody] UpdateMemberRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized();
        }

        var memberId = await memberQueryService.GetMemberIdByUserIdAsync(userId, cancellationToken);
        if (memberId is null)
        {
            return NotFound();
        }

        var command = new UpdateMemberCommand(
            request.LastName,
            request.FirstName,
            request.MiddleName,
            request.SuffixId,
            request.DateOfBirth,
            request.PlaceOfBirth,
            request.CivilStatusId,
            request.JoiningReason,
            request.PositionDesignation,
            request.OfficeUnitId,
            request.PermanentAppointmentDate);

        var requestId = await submissionService.SubmitAsync(memberId.Value, userId, command, cancellationToken);

        return Ok(new SubmitMemberUpdateRequestResponse { Id = requestId });
    }

    [HttpGet]
    [Authorize(Policy = Permission.Membership.ViewSelf)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized();
        }

        var memberId = await memberQueryService.GetMemberIdByUserIdAsync(userId, cancellationToken);
        if (memberId is null)
        {
            return NotFound();
        }

        var requests = await updateRequestQueryService.ListByMemberIdAsync(memberId.Value, cancellationToken);

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
    [Authorize(Policy = Permission.Membership.ViewSelf)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized();
        }

        var memberId = await memberQueryService.GetMemberIdByUserIdAsync(userId, cancellationToken);
        if (memberId is null)
        {
            return NotFound();
        }

        // Every request for this member, not just this id, so a member can
        // never observe (even via 404-vs-200 timing) another member's
        // update request by guessing its id.
        var request = await updateRequestQueryService.GetByIdAsync(id, cancellationToken);
        if (request is null || request.MemberId != memberId.Value)
        {
            return NotFound();
        }

        return Ok(new MemberUpdateRequestDetailResponse
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
        });
    }
}
