using Microsoft.Extensions.Logging;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed class BlueMovieLoader
{
    public const string FileName = "blue_movie_data.json";

    public Task<IReadOnlyList<MovieData>> LoadAsync(
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var bluePath = PathHelper.GetDataPath(GameEra.Blue);
        return Ac15MovieLoader.LoadFromFileAsync(
            Path.Combine(bluePath, FileName),
            BlueGameDataPaths.MovieDirectory,
            nameof(GameEra.Blue),
            logger,
            cancellationToken);
    }
}
