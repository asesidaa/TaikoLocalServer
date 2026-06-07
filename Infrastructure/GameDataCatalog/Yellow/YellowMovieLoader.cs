using Microsoft.Extensions.Logging;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

public sealed class YellowMovieLoader
{
    public const string FileName = "yellow_movie_data.json";

    public Task<IReadOnlyList<MovieData>> LoadAsync(
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var yellowPath = PathHelper.GetDataPath(GameEra.Yellow);
        return Ac15MovieLoader.LoadFromFileAsync(
            Path.Combine(yellowPath, FileName),
            YellowGameDataPaths.MovieDirectory,
            nameof(GameEra.Yellow),
            logger,
            cancellationToken);
    }
}
