using Bimss.Application.Membership;
using Microsoft.Extensions.Options;

namespace Bimss.Infrastructure.Membership;

// Local-disk implementation of IMemberDocumentStorage. Storage keys are
// always server-generated GUIDs (never derived from caller input), so path
// traversal via a crafted storage key is not possible.
public sealed class LocalFileMemberDocumentStorage : IMemberDocumentStorage
{
    private readonly string _rootPath;

    public LocalFileMemberDocumentStorage(IOptions<MemberDocumentStorageOptions> options)
    {
        _rootPath = options.Value.RootPath;
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Stream content, CancellationToken cancellationToken)
    {
        var storageKey = Guid.NewGuid().ToString("N");
        var path = Path.Combine(_rootPath, storageKey);

        await using var fileStream = File.Create(path);
        await content.CopyToAsync(fileStream, cancellationToken);

        return storageKey;
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken)
    {
        Stream stream = File.OpenRead(GetPath(storageKey));
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        var path = GetPath(storageKey);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private string GetPath(string storageKey) => Path.Combine(_rootPath, storageKey);
}
