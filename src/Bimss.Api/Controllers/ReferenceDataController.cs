using Bimss.Application.Membership;
using Bimss.Contracts.Membership;
using Bimss.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bimss.Api.Controllers;

// Gated by the combined ReferenceDataReadPolicy (Manage OR ManageSelf), not
// a single Permission — this is shared taxonomy the officer-facing admin
// forms and the member self-service edit form (BIMSS-042) both need to
// populate a Select from; it isn't member-specific or sensitive.
[ApiController]
[Route("api/reference-data")]
[Authorize(Policy = AuthorizationPolicies.ReferenceDataRead)]
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

    [HttpGet("member-status-reasons")]
    public async Task<IActionResult> ListMemberStatusReasons(CancellationToken cancellationToken)
        => Ok(await MapAsync(referenceDataQueryService.ListMemberStatusReasonsAsync(cancellationToken)));

    private static async Task<IEnumerable<ReferenceDataItemResponse>> MapAsync(
        Task<IReadOnlyList<ReferenceDataSummary>> itemsTask)
    {
        var items = await itemsTask;
        return items.Select(item => new ReferenceDataItemResponse { Id = item.Id, Code = item.Code, Name = item.Name });
    }
}
