using System.ComponentModel.DataAnnotations;

namespace Bimss.Contracts.Diagnostics;

/// <summary>
/// Demonstrates the DataAnnotations-at-the-API-boundary convention (BIMSS-009).
/// Not a real business contract — see DiagnosticsController.ValidateSample.
/// </summary>
public class ValidationCheckRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 150)]
    public int Age { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}
