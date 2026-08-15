using System.Text;
using Bimss.Infrastructure.Membership;
using Microsoft.Extensions.Options;

namespace Bimss.IntegrationTests.Membership;

public class LocalFileMemberDocumentStorageTests : IDisposable
{
    private readonly string _rootPath = Path.Combine(Path.GetTempPath(), "bimss-tests", Guid.NewGuid().ToString());

    [Fact]
    public async Task SaveAsync_ThenOpenReadAsync_RoundTripsContent()
    {
        var storage = CreateStorage();
        var originalBytes = Encoding.UTF8.GetBytes("synthetic document content");

        string storageKey;
        await using (var writeStream = new MemoryStream(originalBytes))
        {
            storageKey = await storage.SaveAsync(writeStream, CancellationToken.None);
        }

        Assert.False(string.IsNullOrWhiteSpace(storageKey));

        await using var readStream = await storage.OpenReadAsync(storageKey, CancellationToken.None);
        using var reader = new MemoryStream();
        await readStream.CopyToAsync(reader);

        Assert.Equal(originalBytes, reader.ToArray());
    }

    [Fact]
    public async Task DeleteAsync_RemovesTheSavedFile()
    {
        var storage = CreateStorage();
        string storageKey;
        await using (var writeStream = new MemoryStream(Encoding.UTF8.GetBytes("synthetic document content")))
        {
            storageKey = await storage.SaveAsync(writeStream, CancellationToken.None);
        }

        await storage.DeleteAsync(storageKey, CancellationToken.None);

        await Assert.ThrowsAsync<FileNotFoundException>(() => storage.OpenReadAsync(storageKey, CancellationToken.None));
    }

    [Fact]
    public async Task SaveAsync_GeneratesDistinctStorageKeys_ForEachSave()
    {
        var storage = CreateStorage();

        string firstKey;
        await using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("first")))
        {
            firstKey = await storage.SaveAsync(stream, CancellationToken.None);
        }

        string secondKey;
        await using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("second")))
        {
            secondKey = await storage.SaveAsync(stream, CancellationToken.None);
        }

        Assert.NotEqual(firstKey, secondKey);
    }

    private LocalFileMemberDocumentStorage CreateStorage()
    {
        var options = Options.Create(new MemberDocumentStorageOptions { RootPath = _rootPath });
        return new LocalFileMemberDocumentStorage(options);
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
        {
            Directory.Delete(_rootPath, recursive: true);
        }

        GC.SuppressFinalize(this);
    }
}
