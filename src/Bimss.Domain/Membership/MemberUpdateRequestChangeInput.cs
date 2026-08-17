namespace Bimss.Domain.Membership;

// Bundles one proposed field change so MemberUpdateRequest's constructor
// takes a collection of these instead of parallel arrays. Which fields are
// actually submittable is BIMSS-042's concern (the submission use case) —
// this schema stays field-agnostic, same reasoning as
// MemberImportStagingFields staying a flat bag of raw values.
public sealed record MemberUpdateRequestChangeInput(string FieldName, string? OldValue, string? NewValue);
