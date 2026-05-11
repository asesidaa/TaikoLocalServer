using TaikoLocalServer.Application.Catalog.Green;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal sealed class GreenMusicInfoLoader
{
    public Task<IReadOnlyDictionary<uint, GreenMusicInfoEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyDictionary<uint, GreenMusicInfoEntry> empty = new Dictionary<uint, GreenMusicInfoEntry>();
        return Task.FromResult(empty);
    }
}
