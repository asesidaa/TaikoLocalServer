using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenTuningLoader
{
    public Task<IReadOnlyDictionary<string, GreenStarSet>> LoadAsync(CancellationToken cancellationToken)
    {
        return LoadFromFileAsync(GreenGameDataPaths.TuningBin, cancellationToken);
    }

    public static async Task<IReadOnlyDictionary<string, GreenStarSet>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var stars = await Ac15TuningLoader.LoadFromFileAsync(path, nameof(GameEra.Green), cancellationToken);
        return stars.ToDictionary(
            pair => pair.Key,
            pair => Map(pair.Value),
            StringComparer.Ordinal);
    }

    private static GreenStarSet Map(Ac15StarSet starSet)
        => new(starSet.Easy, starSet.Normal, starSet.Hard, starSet.Oni, starSet.Ura);
}
