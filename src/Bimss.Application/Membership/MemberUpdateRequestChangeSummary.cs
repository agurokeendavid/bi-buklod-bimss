namespace Bimss.Application.Membership;

public sealed record MemberUpdateRequestChangeSummary(Guid Id, string FieldName, string? OldValue, string? NewValue);
