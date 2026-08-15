namespace Bimss.Infrastructure.Identity;

// Unlike AuditEvent/MemberStatusHistory, this has a real FK to AspNetUsers
// (cascade delete) — a refresh token is meaningless without its user and
// has no audit/history purpose to outlive it.
public class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    // SHA-256 hash of the raw token — the raw value is never persisted.
    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset? RevokedAtUtc { get; set; }

    public ApplicationUser? User { get; set; }
}
