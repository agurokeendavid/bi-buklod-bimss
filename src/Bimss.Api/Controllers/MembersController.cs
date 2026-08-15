using System.Security.Claims;
using Bimss.Application.Membership;
using Bimss.Contracts.Membership;
using Bimss.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bimss.Api.Controllers;

[ApiController]
[Route("api/members")]
// No class-level [Authorize] — Verify uses Permission.Membership.Verify
// while every other action uses Permission.Membership.Manage. Stacking a
// class-level policy with a different method-level policy would require
// BOTH to succeed (ASP.NET Core AND-combines Authorize attributes), which
// would wrongly require Manage for Verify too. Each action states its own
// policy explicitly instead.
public class MembersController(
    IMemberQueryService memberQueryService,
    MemberCreationService memberCreationService,
    MemberProfileUpdateService memberProfileUpdateService,
    MemberStatusTransitionService memberStatusTransitionService)
    : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permission.Membership.Manage)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var members = await memberQueryService.ListAsync(cancellationToken);

        var response = members.Select(member => new MemberSummaryResponse
        {
            Id = member.Id,
            LastName = member.LastName,
            FirstName = member.FirstName,
            MiddleName = member.MiddleName,
            Status = member.Status.ToString(),
            EmployeeNumber = member.EmployeeNumber,
        });

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permission.Membership.Manage)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var member = await memberQueryService.GetByIdAsync(id, cancellationToken);
        if (member is null)
        {
            return NotFound();
        }

        return Ok(ToDetailResponse(member));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permission.Membership.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMemberRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedActorUserId)
            ? parsedActorUserId
            : (Guid?)null;

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

        await memberProfileUpdateService.UpdateAsync(id, command, actorUserId, cancellationToken);

        var member = await memberQueryService.GetByIdAsync(id, cancellationToken);
        return Ok(ToDetailResponse(member!));
    }

    private static MemberDetailResponse ToDetailResponse(MemberDetail member) => new()
    {
        Id = member.Id,
        LastName = member.LastName,
        FirstName = member.FirstName,
        MiddleName = member.MiddleName,
        SuffixId = member.SuffixId,
        DateOfBirth = member.DateOfBirth,
        PlaceOfBirth = member.PlaceOfBirth,
        CivilStatusId = member.CivilStatusId,
        JoiningReason = member.JoiningReason,
        Status = member.Status.ToString(),
        EmployeeNumber = member.EmployeeNumber,
        PositionDesignation = member.PositionDesignation,
        OfficeUnitId = member.OfficeUnitId,
        PermanentAppointmentDate = member.PermanentAppointmentDate,
    };

    [HttpPost]
    [Authorize(Policy = Permission.Membership.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateMemberRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedActorUserId)
            ? parsedActorUserId
            : (Guid?)null;

        var command = new CreateMemberCommand(
            request.LastName,
            request.FirstName,
            request.MiddleName,
            request.SuffixId,
            request.DateOfBirth,
            request.PlaceOfBirth,
            request.CivilStatusId,
            request.JoiningReason,
            request.EmployeeNumber,
            request.PositionDesignation,
            request.OfficeUnitId,
            request.PermanentAppointmentDate);

        var result = await memberCreationService.CreateAsync(command, actorUserId, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.MemberId }, new CreateMemberResponse { Id = result.MemberId });
    }

    [HttpPost("{id:guid}/verify")]
    [Authorize(Policy = Permission.Membership.Verify)]
    public async Task<IActionResult> Verify(Guid id, [FromBody] VerifyMemberRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedActorUserId)
            ? parsedActorUserId
            : (Guid?)null;

        await memberStatusTransitionService.VerifyAsync(id, actorUserId, request.Remarks, cancellationToken);

        var member = await memberQueryService.GetByIdAsync(id, cancellationToken);
        return Ok(ToDetailResponse(member!));
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = Permission.Membership.Manage)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateMemberRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedActorUserId)
            ? parsedActorUserId
            : (Guid?)null;

        await memberStatusTransitionService.DeactivateAsync(id, request.ReasonId, actorUserId, request.Remarks, cancellationToken);

        var member = await memberQueryService.GetByIdAsync(id, cancellationToken);
        return Ok(ToDetailResponse(member!));
    }

    [HttpPost("{id:guid}/reactivate")]
    [Authorize(Policy = Permission.Membership.Manage)]
    public async Task<IActionResult> Reactivate(Guid id, [FromBody] ReactivateMemberRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedActorUserId)
            ? parsedActorUserId
            : (Guid?)null;

        await memberStatusTransitionService.ReactivateAsync(id, actorUserId, request.Remarks, cancellationToken);

        var member = await memberQueryService.GetByIdAsync(id, cancellationToken);
        return Ok(ToDetailResponse(member!));
    }
}
