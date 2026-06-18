using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15CatalogSnapshotFactory
{
    public static Ac15CatalogSnapshot FromSource(Ac15CatalogProjectionSource source)
        => new(
            source.SongHashVersion,
            source.SongNoesInFileOrder,
            source.EventFolders,
            source.Telops,
            source.RecommendSong,
            source.RecommendBestSongs,
            source.ItemShopCatalog,
            source.TaikojukuPacks);

    public static Ac15CatalogSnapshot FromBlue(IBlueCatalog blue)
        => FromSource(new Ac15CatalogProjectionSource(
            blue.SongHashVersion,
            blue.MusicInfoFileOrder.Select(song => song.SongNo).ToArray(),
            blue.EventFolders,
            blue.Telops,
            blue.Recommend.RecommendSong,
            blue.Recommend.RecommendBestSongs.ToArray(),
            blue.ItemShopCatalog,
            blue.TaikojukuFileOrder));

    public static Ac15CatalogSnapshot FromGreen(IGreenCatalog green)
        => FromSource(new Ac15CatalogProjectionSource(
            green.SongHashVersion,
            green.MusicInfoFileOrder.Select(song => song.SongNo).ToArray(),
            green.EventFolders,
            green.Telops,
            green.Recommend.RecommendSong,
            green.Recommend.RecommendBestSongs.ToArray(),
            green.ItemShopCatalog,
            green.TaikojukuFileOrder));

    public static Ac15CatalogSnapshot FromYellow(IYellowCatalog yellow)
        => FromSource(new Ac15CatalogProjectionSource(
            yellow.SongHashVersion,
            yellow.MusicInfoFileOrder.Select(song => song.SongNo).ToArray(),
            yellow.EventFolders,
            yellow.Telops,
            yellow.Recommend.RecommendSong,
            yellow.Recommend.RecommendBestSongs.ToArray(),
            yellow.ItemShopCatalog,
            yellow.TaikojukuFileOrder));

    public static Ac15CatalogSnapshot FromRed(IRedCatalog red)
        => FromSource(new Ac15CatalogProjectionSource(
            red.SongHashVersion,
            red.MusicInfoFileOrder.Select(song => song.SongNo).ToArray(),
            red.EventFolders,
            red.Telops,
            red.Recommend.RecommendSong,
            red.Recommend.RecommendBestSongs.ToArray(),
            Ac15ItemShopCatalog.Disabled,
            red.TaikojukuFileOrder));

    public static Ac15CatalogSnapshot FromWhite(IWhiteCatalog white)
        => FromSource(new Ac15CatalogProjectionSource(
            white.SongHashVersion,
            white.MusicInfoFileOrder.Select(song => song.SongNo).ToArray(),
            white.EventFolders,
            white.Telops,
            white.Recommend.RecommendSong,
            white.Recommend.RecommendBestSongs.ToArray(),
            Ac15ItemShopCatalog.Disabled,
            white.TaikojukuFileOrder));
}
