using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog;

public sealed class FileGameDataCatalog : IGameDataCatalog
{
    private readonly IReadOnlyDictionary<GameEra, IEraGameDataCatalog> catalogs;
    private readonly ILogger logger;
    private readonly ConcurrentDictionary<GameEra, EraLoadResult> results = new();

    public FileGameDataCatalog(IEnumerable<IEraGameDataCatalog> eras, ILogger<FileGameDataCatalog>? logger = null)
    {
        catalogs = eras.ToDictionary(catalog => catalog.Era);
        this.logger = logger ?? NullLogger<FileGameDataCatalog>.Instance;
    }

    public IReadOnlyDictionary<GameEra, EraLoadResult> LoadResults => results;

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
        if (catalogs.ContainsKey(GameEra.Nijiiro))
        {
            await ReloadAsync(GameEra.Nijiiro, cancellationToken);
        }

        await Task.WhenAll(catalogs.Keys
            .Where(era => era != GameEra.Nijiiro)
            .Select(era => ReloadAsync(era, cancellationToken)));
    }

    // A game whose files are missing or broken doesn't stop the server: its catalogue keeps what it had (empty at
    // start-up) and the error shows on Server Management › Status.
    // shortcut: a reload swaps the catalogue's collections one after another, so a request during it may see old and
    // new together; fine for an admin's Check again, make it one swap if reloads become frequent.
    public async Task<EraLoadResult> ReloadAsync(GameEra era, CancellationToken cancellationToken = default)
    {
        string? error = null;
        try
        {
            await For(era).InitializeAsync(cancellationToken);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            error = e.Message;
            logger.LogError(e, "{Era}: game data didn't load", era);
        }

        return results[era] = new EraLoadResult(era, DateTime.UtcNow, error);
    }
}
