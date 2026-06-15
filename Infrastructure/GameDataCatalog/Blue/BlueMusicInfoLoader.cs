using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed record BlueMusicInfoLoadResult(
    uint SongHashVersion,
    IReadOnlyList<BlueMusicInfoEntry> Entries);

public sealed class BlueMusicInfoLoader
{
    public Task<BlueMusicInfoLoadResult> LoadAsync(CancellationToken cancellationToken)
    {
        return LoadFromFileAsync(BlueGameDataPaths.MusicInfoXml, cancellationToken);
    }

    public static async Task<BlueMusicInfoLoadResult> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var result = await Ac15MusicInfoLoader.LoadFromFileAsync(path, cancellationToken);
        return new BlueMusicInfoLoadResult(
            result.SongHashVersion,
            result.Entries.Select(Map).ToArray());
    }

    private static BlueMusicInfoEntry Map(Ac15MusicInfoEntry entry) => new()
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
