using Bimss.Domain.Auditing;

namespace Bimss.Application.Auditing;

public sealed record AuditEntry
{
    public Guid? ActorUserId { get; }

    public string Action { get; }

    public string ObjectType { get; }

    public string ObjectId { get; }

    public AuditResult Result { get; }

    public string? Remarks { get; }

    public IReadOnlyDictionary<string, string>? Metadata { get; }

    public AuditEntry(
        Guid? actorUserId,
        string action,
        string objectType,
        string objectId,
        AuditResult result,
        string? remarks = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectId);

        ActorUserId = actorUserId;
        Action = action;
        ObjectType = objectType;
        ObjectId = objectId;
        Result = result;
        Remarks = remarks;
        Metadata = metadata;
    }
}
