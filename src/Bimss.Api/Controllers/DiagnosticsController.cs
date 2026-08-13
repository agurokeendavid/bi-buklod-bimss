using Bimss.Domain.Authorization;
using Bimss.Domain.Exceptions;
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

    /// <summary>
    /// Exercises the global exception handling pipeline on demand. Anonymous
    /// and exists purely so tests can assert typed exceptions map to the
    /// correct HTTP status/ProblemDetails shape without needing a real
    /// business failure to trigger them.
    /// </summary>
    [HttpGet("throw")]
    [AllowAnonymous]
    public IActionResult Throw([FromQuery] string type)
    {
        throw type switch
        {
            "notfound" => new NotFoundException("Sample object was not found."),
            "conflict" => new ConflictException("Sample object is in conflict."),
            "forbidden" => new ForbiddenException("Sample action is forbidden."),
            "validation" => new DomainValidationException(
                "Sample validation failed.",
                new Dictionary<string, string[]> { ["field"] = ["is required"] }),
            _ => new InvalidOperationException("Sample unexpected failure with sensitive internal detail: password=hunter2"),
        };
    }
}
