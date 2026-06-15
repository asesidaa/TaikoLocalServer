using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed record GreenMusicInfoLoadResult(
    uint SongHashVersion,
    IReadOnlyList<GreenMusicInfoEntry> Entries);

public sealed class GreenMusicInfoLoader
{
    public Task<GreenMusicInfoLoadResult> LoadAsync(CancellationToken cancellationToken)
    {
        return LoadFromFileAsync(GreenGameDataPaths.MusicInfoXml, cancellationToken);
    }

    public static async Task<GreenMusicInfoLoadResult> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var result = await Ac15MusicInfoLoader.LoadFromFileAsync(path, cancellationToken);
        return new GreenMusicInfoLoadResult(
            result.SongHashVersion,
            result.Entries.Select(Map).ToArray());
    }

    private static GreenMusicInfoEntry Map(Ac15MusicInfoEntry entry) => new()
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
