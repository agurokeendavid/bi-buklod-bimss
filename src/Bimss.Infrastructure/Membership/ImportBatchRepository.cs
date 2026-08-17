using Bimss.Application.Membership;
using Bimss.Domain.Membership;
using Bimss.Infrastructure.Persistence;

namespace Bimss.Infrastructure.Membership;

public sealed class ImportBatchRepository(BimssDbContext dbContext) : IImportBatchRepository
{
    public async Task AddBatchWithRowsAsync(
        ImportBatch batch, IReadOnlyCollection<MemberImportStaging> rows, CancellationToken cancellationToken)
    {
        dbContext.ImportBatches.Add(batch);
        dbContext.MemberImportStagingRows.AddRange(rows);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
