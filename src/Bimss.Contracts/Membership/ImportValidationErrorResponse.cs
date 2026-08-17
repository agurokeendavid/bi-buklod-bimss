namespace Bimss.Contracts.Membership;

public class ImportValidationErrorResponse
{
    public Guid Id { get; set; }

    public Guid? MemberImportStagingId { get; set; }

    public string? FieldName { get; set; }

    public string Severity { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
