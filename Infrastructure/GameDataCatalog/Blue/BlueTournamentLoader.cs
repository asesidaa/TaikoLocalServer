using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

internal sealed class BlueTournamentLoader
{
    public Task<IReadOnlyDictionary<uint, Ac15TournamentEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyDictionary<uint, Ac15TournamentEntry> empty = new Dictionary<uint, Ac15TournamentEntry>();
        return Task.FromResult(empty);
    }
}
