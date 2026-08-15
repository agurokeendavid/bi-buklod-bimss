using Bimss.Application.Membership;
using Bimss.Contracts.Membership;
using Bimss.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bimss.Api.Controllers;

[ApiController]
[Route("api/reference-data")]
[Authorize(Policy = Permission.Membership.Manage)]
public class ReferenceDataController(IReferenceDataQueryService referenceDataQueryService) : ControllerBase
{
    [HttpGet("civil-statuses")]
    public async Task<IActionResult> ListCivilStatuses(CancellationToken cancellationToken)
        => Ok(await MapAsync(referenceDataQueryService.ListCivilStatusesAsync(cancellationToken)));

    [HttpGet("suffixes")]
    public async Task<IActionResult> ListSuffixes(CancellationToken cancellationToken)
        => Ok(await MapAsync(referenceDataQueryService.ListSuffixesAsync(cancellationToken)));

    [HttpGet("office-units")]
    public async Task<IActionResult> ListOfficeUnits(CancellationToken cancellationToken)
        => Ok(await MapAsync(referenceDataQueryService.ListOfficeUnitsAsync(cancellationToken)));

    private static async Task<IEnumerable<ReferenceDataItemResponse>> MapAsync(
        Task<IReadOnlyList<ReferenceDataSummary>> itemsTask)
    {
        var items = await itemsTask;
        return items.Select(item => new ReferenceDataItemResponse { Id = item.Id, Code = item.Code, Name = item.Name });
    }
}
