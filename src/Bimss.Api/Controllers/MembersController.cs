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
}
