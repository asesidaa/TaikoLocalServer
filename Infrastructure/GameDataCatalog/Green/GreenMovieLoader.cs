using Microsoft.Extensions.Logging;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenMovieLoader
{
    public Task<IReadOnlyList<MovieData>> LoadAsync(
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var greenPath = PathHelper.GetDataPath(GameEra.Green);
        return LoadFromFileAsync(
            Path.Combine(greenPath, "movie_data.json"),
            Path.Combine(greenPath, "data", "movie"),
            logger,
            cancellationToken);
    }

    public static Task<IReadOnlyList<MovieData>> LoadFromFileAsync(
        string configPath,
        string movieDirectory,
        ILogger logger,
        CancellationToken cancellationToken)
        => Ac15MovieLoader.LoadFromFileAsync(
            configPath,
            movieDirectory,
            nameof(GameEra.Green),
            logger,
            cancellationToken);
}
