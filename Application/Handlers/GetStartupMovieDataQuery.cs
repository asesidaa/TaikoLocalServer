using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Settings;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetStartupMovieDataQuery(uint HddVer) : IRequest<IReadOnlyList<MovieData>>;

public sealed class GetStartupMovieDataQueryHandler(
    IGameDataCatalog gameDataService,
    ILogger<GetStartupMovieDataQueryHandler> logger,
    IOptions<ServerSettings> settings)
    : IRequestHandler<GetStartupMovieDataQuery, IReadOnlyList<MovieData>>
{
    private readonly ServerSettings settings = settings.Value;

    public ValueTask<IReadOnlyList<MovieData>> Handle(
        GetStartupMovieDataQuery query,
        CancellationToken cancellationToken)
    {
        var era = ResolveEra(query.HddVer);
        if (era is null)
        {
            return ValueTask.FromResult((IReadOnlyList<MovieData>)[]);
        }

        var movies = era.Value switch
        {
            GameEra.White => gameDataService.White().Movies,
            GameEra.Murasaki => gameDataService.Murasaki().Movies,
            GameEra.Kimidori => gameDataService.Kimidori().Movies,
            GameEra.Red => gameDataService.Red().Movies,
            GameEra.Blue => gameDataService.Blue().Movies,
            GameEra.Green => gameDataService.Green().Movies,
            GameEra.Yellow => gameDataService.Yellow().Movies,
            GameEra.Nijiiro => gameDataService.Nijiiro()
                .GetMovieDataDictionary()
                .Values
                .OrderBy(movie => movie.MovieId)
                .ToArray(),
            _ => []
        };

        return ValueTask.FromResult(movies);
    }

    private GameEra? ResolveEra(uint hddVer)
    {
        var requestedEra = (hddVer / 100) switch
        {
            7 => GameEra.White,
            6 => GameEra.Murasaki,
            5 => GameEra.Kimidori,
            8 => GameEra.Red,
            9 => GameEra.Yellow,
            10 => GameEra.Blue,
            11 => GameEra.Green,
            12 => GameEra.Nijiiro,
            _ => (GameEra?)null
        };

        if (requestedEra is not null)
        {
            if (IsEnabled(requestedEra.Value))
            {
                return requestedEra.Value;
            }

            logger.LogWarning(
                "Startup auth HDD version {HddVer} resolved to disabled era {Era}; no movie permissions will be sent.",
                hddVer,
                requestedEra.Value);
            return null;
        }

        var enabledEras = GetEnabledEras();
        if (enabledEras.Count == 1)
        {
            logger.LogWarning(
                "Startup auth HDD version {HddVer} is unknown; using the only enabled era {Era}.",
                hddVer,
                enabledEras[0]);
            return enabledEras[0];
        }

        logger.LogWarning(
            "Startup auth HDD version {HddVer} is unknown and {EnabledEraCount} eras are enabled; no movie permissions will be sent.",
            hddVer,
            enabledEras.Count);
        return null;
    }

    private bool IsEnabled(GameEra era)
    {
        return settings.Eras.TryGetValue(era.ToString(), out var eraSettings)
               && eraSettings.Enabled;
    }

    private IReadOnlyList<GameEra> GetEnabledEras()
    {
        return settings.Eras
            .Where(pair => pair.Value.Enabled)
            .Select(pair => Enum.TryParse<GameEra>(pair.Key, ignoreCase: true, out var era)
                ? era
                : (GameEra?)null)
            .Where(era => era is not null)
            .Select(era => era!.Value)
            .Distinct()
            .Order()
            .ToArray();
    }
}
