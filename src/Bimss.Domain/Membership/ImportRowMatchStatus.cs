namespace Bimss.Domain.Membership;

public enum ImportRowMatchStatus
{
    NotEvaluated = 0,
    NoMatch = 1,
    PossibleDuplicate = 2,
    ConfirmedDuplicate = 3,
}
