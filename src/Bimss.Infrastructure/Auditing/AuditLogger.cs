using System.Text.Json;
using Bimss.Application.Auditing;
using Bimss.Infrastructure.Persistence;

namespace Bimss.Infrastructure.Auditing;

public class AuditLogger(BimssDbContext dbContext, TimeProvider timeProvider) : IAuditLogger
{
    public async Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        var auditEvent = new AuditEvent
        {
            Id = Guid.NewGuid(),
            ActorUserId = entry.ActorUserId,
            Action = entry.Action,
            ObjectType = entry.ObjectType,
            ObjectId = entry.ObjectId,
            TimestampUtc = timeProvider.GetUtcNow(),
            Result = entry.Result,
            Remarks = entry.Remarks,
            MetadataJson = entry.Metadata is { Count: > 0 } metadata ? JsonSerializer.Serialize(metadata) : null,
        };

        dbContext.AuditEvents.Add(auditEvent);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
