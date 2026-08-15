namespace Bimss.Application.Membership;

/// <summary>
/// Stores and retrieves the file bytes behind a MemberDocument's metadata.
/// Implementations must generate server-side storage names and must never
/// trust caller-supplied file names or paths
/// (docs/SECURITY_AND_PRIVACY.md's "File uploads" rules).
/// </summary>
public interface IMemberDocumentStorage
{
    /// <summary>Saves the content and returns a server-generated storage key.</summary>
    Task<string> SaveAsync(Stream content, CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);
}
