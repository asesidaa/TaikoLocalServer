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

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (catalogs.TryGetValue(GameEra.Nijiiro, out var nijiiroCatalog))
        {
            await nijiiroCatalog.InitializeAsync(cancellationToken);
        }

        await Task.WhenAll(catalogs
            .Where(pair => pair.Key != GameEra.Nijiiro)
            .Select(pair => pair.Value.InitializeAsync(cancellationToken)));
    }
}
