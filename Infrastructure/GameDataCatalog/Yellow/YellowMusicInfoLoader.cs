using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Yellow;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

public sealed record YellowMusicInfoLoadResult(
    uint SongHashVersion,
    IReadOnlyList<YellowMusicInfoEntry> Entries);

public sealed class YellowMusicInfoLoader
{
    public Task<YellowMusicInfoLoadResult> LoadAsync(CancellationToken cancellationToken)
        => LoadFromFileAsync(YellowGameDataPaths.MusicInfoXml, cancellationToken);

    public static async Task<YellowMusicInfoLoadResult> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var result = await Ac15MusicInfoLoader.LoadFromFileAsync(path, cancellationToken);
        return new YellowMusicInfoLoadResult(
            result.SongHashVersion,
            result.Entries.Select(Map).ToArray());
    }

    private static YellowMusicInfoEntry Map(Ac15MusicInfoEntry entry) => new()
    {
        MusicId = entry.MusicId,
        SongNo = entry.SongNo,
        NewRelease = entry.NewRelease,
        IsSecret = entry.IsSecret,
        IsPapaMama = entry.IsPapaMama,
        HasExtreme = entry.HasExtreme,
        PartsSet = entry.PartsSet,
        WaiwaiPartsSet = entry.WaiwaiPartsSet,
        Title = entry.Title,
        GenreName = entry.GenreName,
        CategoryId = entry.CategoryId,
        DemoPlay = entry.DemoPlay,
        Tags = entry.Tags,
        FileOrder = entry.FileOrder
    };
}
