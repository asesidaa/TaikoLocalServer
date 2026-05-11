using TaikoLocalServer.Application.Catalog.Green;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal sealed class GreenTournamentLoader
{
    public Task<IReadOnlyDictionary<uint, GreenTournamentEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyDictionary<uint, GreenTournamentEntry> empty = new Dictionary<uint, GreenTournamentEntry>();
        return Task.FromResult(empty);
    }
}
