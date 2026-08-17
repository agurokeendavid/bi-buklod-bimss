namespace Bimss.Application.Membership;

public sealed record MyContactDetail(
    Guid MemberId,
    string? Landline,
    string? MobileNumber,
    string? Email,
    string? PresentAddress,
    string? PermanentAddress);
