namespace Bimss.Contracts.Membership;

public class MemberUpdateRequestChangeResponse
{
    public Guid Id { get; set; }

    public string FieldName { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }
}
