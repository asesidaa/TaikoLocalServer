namespace TaikoLocalServer.Application.Abstractions;

public interface IGameDataCatalog
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    // Members re-added after Catalog/ + ServerData/ are populated in Phases 2.5, 2.6.
}
