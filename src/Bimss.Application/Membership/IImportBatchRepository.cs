using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

// Narrow, use-case-specific port for the import pipeline — same reasoning as
// IMemberRepository (AGENTS.md: "Do not create a generic repository
// abstraction over EF Core unless there is a demonstrated need"). Grows
// incrementally as later import tasks need more, matching how
// IMemberRepository grew across BIMSS-022/030/032.
public interface IImportBatchRepository
{
    // BIMSS-034: Excel ingestion — persists a freshly created batch and its
    // staged rows as one unit of work.
    Task AddBatchWithRowsAsync(
        ImportBatch batch, IReadOnlyCollection<MemberImportStaging> rows, CancellationToken cancellationToken);
}
