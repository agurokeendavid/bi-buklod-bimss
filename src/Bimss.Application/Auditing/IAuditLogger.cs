namespace Bimss.Application.Auditing;

/// <summary>
/// Records that a business action happened. Call explicitly at the point the
/// action occurs — never derive audit entries from a generic SaveChanges diff.
/// <see cref="AuditEntry.Metadata"/> must never contain beneficiary data,
/// ballot contents, full addresses, or other sensitive data barred by
/// docs/SECURITY_AND_PRIVACY.md's logging rules — only safe, minimal
/// before/after metadata.
/// </summary>
public interface IAuditLogger
{
    Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}
