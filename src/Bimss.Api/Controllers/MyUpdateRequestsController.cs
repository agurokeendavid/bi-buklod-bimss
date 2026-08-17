using System.Security.Claims;
using Bimss.Application.Membership;
using Bimss.Contracts.Membership;
using Bimss.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bimss.Api.Controllers;

// Member self-service (Phase 1E) — submitting a profile-change request for
// officer review (docs/DOMAIN_WORKFLOWS.md's "Member profile update"
// workflow). Scoped to Permission.Membership.ManageSelf, not Manage;
// resolves "which member" from the caller's own user id, same as
// MyProfileController — no way to submit a request on another member's
// behalf through this endpoint.
[ApiController]
[Route("api/my/update-requests")]
[Authorize(Policy = Permission.Membership.ManageSelf)]
public class MyUpdateRequestsController(
    IMemberQueryService memberQueryService, MemberUpdateRequestSubmissionService submissionService)
    : ControllerBase
{
    [HttpPost]
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
}
