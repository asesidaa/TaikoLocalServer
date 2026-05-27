using TaikoLocalServer.Application.Catalog.Blue;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

internal sealed class BlueTournamentLoader
{
    public Task<IReadOnlyDictionary<uint, BlueTournamentEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyDictionary<uint, BlueTournamentEntry> empty = new Dictionary<uint, BlueTournamentEntry>();
        return Task.FromResult(empty);
    }
}
