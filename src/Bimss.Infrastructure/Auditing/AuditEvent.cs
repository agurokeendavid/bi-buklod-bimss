using Bimss.Domain.Auditing;

namespace Bimss.Infrastructure.Auditing;

public class AuditEvent
{
    public Guid Id { get; set; }

    public Guid? ActorUserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string ObjectType { get; set; } = string.Empty;

    public string ObjectId { get; set; } = string.Empty;

    public DateTimeOffset TimestampUtc { get; set; }

    public AuditResult Result { get; set; }

    public string? Remarks { get; set; }

    public string? MetadataJson { get; set; }
}
