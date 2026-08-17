using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

public sealed record ImportValidationErrorSummary(
    Guid Id,
    Guid? MemberImportStagingId,
    string? FieldName,
    ImportValidationSeverity Severity,
    string Message);
