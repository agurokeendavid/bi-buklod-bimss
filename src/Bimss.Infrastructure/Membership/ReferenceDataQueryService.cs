using Bimss.Application.Membership;
using Bimss.Domain.Membership.ReferenceData;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bimss.Infrastructure.Membership;

public sealed class ReferenceDataQueryService(BimssDbContext dbContext) : IReferenceDataQueryService
{
    public Task<IReadOnlyList<ReferenceDataSummary>> ListCivilStatusesAsync(CancellationToken cancellationToken)
        => ListActiveAsync(dbContext.CivilStatuses, cancellationToken);

    public Task<IReadOnlyList<ReferenceDataSummary>> ListSuffixesAsync(CancellationToken cancellationToken)
        => ListActiveAsync(dbContext.Suffixes, cancellationToken);

    public Task<IReadOnlyList<ReferenceDataSummary>> ListOfficeUnitsAsync(CancellationToken cancellationToken)
        => ListActiveAsync(dbContext.OfficeUnits, cancellationToken);

    // Only active rows — inactive reference rows are kept for the
    // historical FK integrity of members that already reference them, but
    // shouldn't be selectable for new member creation going forward.
    private static async Task<IReadOnlyList<ReferenceDataSummary>> ListActiveAsync<T>(
        DbSet<T> dbSet, CancellationToken cancellationToken)
        where T : ReferenceDataItem
    {
        return await dbSet
            .Where(item => item.IsActive)
            .OrderBy(item => item.Name)
            .Select(item => new ReferenceDataSummary(item.Id, item.Code, item.Name))
            .ToListAsync(cancellationToken);
    }
}
