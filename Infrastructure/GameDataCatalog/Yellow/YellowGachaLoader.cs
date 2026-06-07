using TaikoLocalServer.Application.Catalog.Yellow;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

internal sealed class YellowGachaLoader
{
    public Task<IReadOnlyDictionary<uint, YellowGachaEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyDictionary<uint, YellowGachaEntry> empty = new Dictionary<uint, YellowGachaEntry>();
        return Task.FromResult(empty);
    }
}
