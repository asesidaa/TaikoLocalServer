using TaikoLocalServer.Application.Catalog.Green;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal sealed class GreenItemShopLoader
{
    public Task<IReadOnlyDictionary<uint, GreenItemShopEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyDictionary<uint, GreenItemShopEntry> empty = new Dictionary<uint, GreenItemShopEntry>();
        return Task.FromResult(empty);
    }
}
