using TaikoLocalServer.Application.Catalog.Green;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal sealed class GreenGachaLoader
{
    public Task<IReadOnlyDictionary<uint, GreenGachaEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyDictionary<uint, GreenGachaEntry> empty = new Dictionary<uint, GreenGachaEntry>();
        return Task.FromResult(empty);
    }
}
