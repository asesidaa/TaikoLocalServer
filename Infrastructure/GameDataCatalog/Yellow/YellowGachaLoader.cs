using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

internal sealed class YellowGachaLoader
{
    public Task<IReadOnlyDictionary<uint, Ac15GachaEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyDictionary<uint, Ac15GachaEntry> empty = new Dictionary<uint, Ac15GachaEntry>();
        return Task.FromResult(empty);
    }
}
