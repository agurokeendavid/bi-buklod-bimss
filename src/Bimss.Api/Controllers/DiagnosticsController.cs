using Bimss.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bimss.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    [HttpGet("authorized-ping")]
    [Authorize(Policy = Permission.Audit.View)]
    public IActionResult AuthorizedPing() => Ok();
}
