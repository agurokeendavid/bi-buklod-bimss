namespace Bimss.Domain.Membership;

public enum ImportBatchStatus
{
    Created = 0,
    Staged = 1,
    Validated = 2,
    Promoted = 3,
    Cancelled = 4,
}
