namespace Bimss.Infrastructure.Authorization;

// Named authorization policies that combine more than one Permission — kept
// separate from AuthorizationServiceCollectionExtensions (whose own name
// collides with Microsoft.Extensions.DependencyInjection's built-in type of
// the same name when referenced explicitly by callers).
public static class AuthorizationPolicies
{
    // Manage OR ManageSelf — shared read access to reference/master data
    // (see AuthorizationServiceCollectionExtensions for why this can't be a
    // single Permission-backed policy).
    public const string ReferenceDataRead = "ReferenceData.Read";
}
