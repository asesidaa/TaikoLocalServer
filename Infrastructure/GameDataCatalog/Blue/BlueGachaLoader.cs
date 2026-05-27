using TaikoLocalServer.Application.Catalog.Blue;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

internal sealed class BlueGachaLoader
{
    public Task<IReadOnlyDictionary<uint, BlueGachaEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyDictionary<uint, BlueGachaEntry> empty = new Dictionary<uint, BlueGachaEntry>();
        return Task.FromResult(empty);
    }
}
