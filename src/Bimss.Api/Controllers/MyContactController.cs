using System.Security.Claims;
using Bimss.Application.Membership;
using Bimss.Contracts.Membership;
using Bimss.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bimss.Api.Controllers;

// Member self-service (Phase 1E) — direct edit of contact info, the one
// profile area docs/DATA_DICTIONARY.md's confirmed decision allows without
// officer review. Read is Permission.Membership.ViewSelf; the update is
// ManageSelf. Resolves "which member" from the caller's own user id, same
// pattern as MyProfileController/MyUpdateRequestsController.
[ApiController]
[Route("api/my/contact")]
public class MyContactController(
    IMemberQueryService memberQueryService, MemberContactSelfServiceUpdateService updateService)
    : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permission.Membership.ViewSelf)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized();
        }

        var contact = await memberQueryService.GetMyContactByUserIdAsync(userId, cancellationToken);
        if (contact is null)
        {
            return NotFound();
        }

        return Ok(new MyContactResponse
        {
            Landline = contact.Landline,
            MobileNumber = contact.MobileNumber,
            Email = contact.Email,
            PresentAddress = contact.PresentAddress,
            PermanentAddress = contact.PermanentAddress,
        });
    }

    [HttpPut]
    [Authorize(Policy = Permission.Membership.ManageSelf)]
    public async Task<IActionResult> Update([FromBody] UpdateMyContactRequest request, CancellationToken cancellationToken)
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

        await updateService.UpdateAsync(
            memberId.Value,
            userId,
            request.Landline,
            request.MobileNumber,
            request.Email,
            request.PresentAddress,
            request.PermanentAddress,
            cancellationToken);

        var updated = await memberQueryService.GetMyContactByUserIdAsync(userId, cancellationToken);
        return Ok(new MyContactResponse
        {
            Landline = updated!.Landline,
            MobileNumber = updated.MobileNumber,
            Email = updated.Email,
            PresentAddress = updated.PresentAddress,
            PermanentAddress = updated.PermanentAddress,
        });
    }
}
