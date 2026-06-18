using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal sealed class GreenGachaLoader
{
    public Task<IReadOnlyDictionary<uint, Ac15GachaEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyDictionary<uint, Ac15GachaEntry> empty = new Dictionary<uint, Ac15GachaEntry>();
        return Task.FromResult(empty);
    }
}
