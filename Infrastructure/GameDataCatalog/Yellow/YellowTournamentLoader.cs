using TaikoLocalServer.Application.Catalog.Yellow;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

internal sealed class YellowTournamentLoader
{
    public Task<IReadOnlyDictionary<uint, YellowTournamentEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyDictionary<uint, YellowTournamentEntry> empty = new Dictionary<uint, YellowTournamentEntry>();
        return Task.FromResult(empty);
    }
}
