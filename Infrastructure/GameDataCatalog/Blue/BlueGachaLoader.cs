using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

internal sealed class BlueGachaLoader
{
    public Task<IReadOnlyDictionary<uint, Ac15GachaEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyDictionary<uint, Ac15GachaEntry> empty = new Dictionary<uint, Ac15GachaEntry>();
        return Task.FromResult(empty);
    }
}
