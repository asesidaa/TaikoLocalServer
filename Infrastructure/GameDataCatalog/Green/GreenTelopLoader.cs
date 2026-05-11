using TaikoLocalServer.Application.Catalog.Green;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal sealed class GreenTelopLoader
{
    public Task<IReadOnlyDictionary<uint, GreenTelopEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyDictionary<uint, GreenTelopEntry> empty = new Dictionary<uint, GreenTelopEntry>();
        return Task.FromResult(empty);
    }
}
