using TaikoLocalServer.Application.Catalog.Green;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal sealed class GreenTaikojukuLoader
{
    public Task<IReadOnlyDictionary<uint, GreenTaikojukuEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyDictionary<uint, GreenTaikojukuEntry> empty = new Dictionary<uint, GreenTaikojukuEntry>();
        return Task.FromResult(empty);
    }
}
