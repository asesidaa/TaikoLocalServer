using TaikoLocalServer.Application.Catalog.Green;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal sealed class GreenEventFolderLoader
{
    public Task<IReadOnlyDictionary<uint, GreenEventFolderEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyDictionary<uint, GreenEventFolderEntry> empty = new Dictionary<uint, GreenEventFolderEntry>();
        return Task.FromResult(empty);
    }
}
