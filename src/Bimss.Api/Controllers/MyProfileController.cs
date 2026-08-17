using System.Security.Claims;
using Bimss.Application.Membership;
using Bimss.Contracts.Membership;
using Bimss.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bimss.Api.Controllers;

// Member self-service (Phase 1E) — deliberately its own controller and
// route, distinct from the officer-facing MembersController. Scoped to
// Permission.Membership.ViewSelf, not Manage; resolves "which member" from
// the caller's own user id rather than a route parameter, so there is no
// way to request another member's profile through this endpoint.
[ApiController]
[Route("api/my/profile")]
[Authorize(Policy = Permission.Membership.ViewSelf)]
public class MyProfileController(IMemberQueryService memberQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized();
        }

        var profile = await memberQueryService.GetMyProfileByUserIdAsync(userId, cancellationToken);
        if (profile is null)
        {
            return NotFound();
        }

        return Ok(new MyProfileResponse
        {
            Id = profile.Id,
            LastName = profile.LastName,
            FirstName = profile.FirstName,
            MiddleName = profile.MiddleName,
            SuffixName = profile.SuffixName,
            DateOfBirth = profile.DateOfBirth,
            PlaceOfBirth = profile.PlaceOfBirth,
            CivilStatusName = profile.CivilStatusName,
            JoiningReason = profile.JoiningReason,
            Status = profile.Status.ToString(),
            EmployeeNumber = profile.EmployeeNumber,
            PositionDesignation = profile.PositionDesignation,
            OfficeUnitName = profile.OfficeUnitName,
            PermanentAppointmentDate = profile.PermanentAppointmentDate,
        });
    }
}
