using Bimss.Application.Membership;
using Bimss.Contracts.Membership;
using Bimss.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bimss.Api.Controllers;

[ApiController]
[Route("api/members")]
public class MembersController(IMemberQueryService memberQueryService) : ControllerBase
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

        return Ok(new MemberDetailResponse
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
        });
    }
}
