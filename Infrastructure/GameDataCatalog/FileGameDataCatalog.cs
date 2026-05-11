using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog;

public sealed class FileGameDataCatalog : IGameDataCatalog
{
    private readonly IReadOnlyDictionary<GameEra, IEraGameDataCatalog> catalogs;

    public FileGameDataCatalog(IEnumerable<IEraGameDataCatalog> eras)
    {
        catalogs = eras.ToDictionary(catalog => catalog.Era);
    }

    public IEraGameDataCatalog For(GameEra era)
    {
        if (!catalogs.TryGetValue(era, out var catalog))
        {
            throw new InvalidOperationException($"Era {era} is not enabled.");
        }

        return catalog;
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
        => Task.WhenAll(catalogs.Values.Select(catalog => catalog.InitializeAsync(cancellationToken)));
}
