using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Application.Catalog.Yellow;

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
            MapTelops(
                blue.Telops,
                entry => entry.TelopId,
                entry => entry.VerupNo,
                entry => entry.StartDatetime,
                entry => entry.EndDatetime,
                entry => entry.Message),
            blue.Recommend.RecommendSong,
            blue.Recommend.RecommendBestSongs.ToArray(),
            MapItemShop(blue.ItemShopCatalog),
            blue.TaikojukuFileOrder.Select(MapTaikojuku).ToArray()));

    public static Ac15CatalogSnapshot FromGreen(IGreenCatalog green)
        => FromSource(new Ac15CatalogProjectionSource(
            green.SongHashVersion,
            green.MusicInfoFileOrder.Select(song => song.SongNo).ToArray(),
            green.EventFolders,
            MapTelops(
                green.Telops,
                entry => entry.TelopId,
                entry => entry.VerupNo,
                entry => entry.StartDatetime,
                entry => entry.EndDatetime,
                entry => entry.Message),
            green.Recommend.RecommendSong,
            green.Recommend.RecommendBestSongs.ToArray(),
            MapItemShop(green.ItemShopCatalog),
            green.TaikojukuFileOrder.Select(MapTaikojuku).ToArray()));

    public static Ac15CatalogSnapshot FromYellow(IYellowCatalog yellow)
        => FromSource(new Ac15CatalogProjectionSource(
            yellow.SongHashVersion,
            yellow.MusicInfoFileOrder.Select(song => song.SongNo).ToArray(),
            yellow.EventFolders,
            MapTelops(
                yellow.Telops,
                entry => entry.TelopId,
                entry => entry.VerupNo,
                entry => entry.StartDatetime,
                entry => entry.EndDatetime,
                entry => entry.Message),
            yellow.Recommend.RecommendSong,
            yellow.Recommend.RecommendBestSongs.ToArray(),
            MapItemShop(yellow.ItemShopCatalog),
            yellow.TaikojukuFileOrder.Select(MapTaikojuku).ToArray()));

    private static Dictionary<uint, Ac15TelopEntry> MapTelops<TEntry>(
        IReadOnlyDictionary<uint, TEntry> telops,
        Func<TEntry, uint> telopId,
        Func<TEntry, uint> verupNo,
        Func<TEntry, string> startDatetime,
        Func<TEntry, string> endDatetime,
        Func<TEntry, string> message)
        => telops.ToDictionary(
            pair => pair.Key,
            pair => new Ac15TelopEntry
            {
                TelopId = telopId(pair.Value),
                VerupNo = verupNo(pair.Value),
                StartDatetime = startDatetime(pair.Value),
                EndDatetime = endDatetime(pair.Value),
                Message = message(pair.Value)
            });

    private static Ac15ItemShopCatalog MapItemShop(BlueItemShopCatalog catalog)
        => catalog.IsEnabled
            ? new Ac15ItemShopCatalog
            {
                IsEnabled = true,
                ActiveSeasonId = catalog.ActiveSeasonId,
                Seasons = catalog.Seasons.ToDictionary(pair => pair.Key, pair => MapSeason(pair.Value))
            }
            : Ac15ItemShopCatalog.Disabled;

    private static Ac15ItemShopCatalog MapItemShop(GreenItemShopCatalog catalog)
        => catalog.IsEnabled
            ? new Ac15ItemShopCatalog
            {
                IsEnabled = true,
                ActiveSeasonId = catalog.ActiveSeasonId,
                Seasons = catalog.Seasons.ToDictionary(pair => pair.Key, pair => MapSeason(pair.Value))
            }
            : Ac15ItemShopCatalog.Disabled;

    private static Ac15ItemShopCatalog MapItemShop(YellowItemShopCatalog catalog)
        => catalog.IsEnabled
            ? new Ac15ItemShopCatalog
            {
                IsEnabled = true,
                ActiveSeasonId = catalog.ActiveSeasonId,
                Seasons = catalog.Seasons.ToDictionary(pair => pair.Key, pair => MapSeason(pair.Value))
            }
            : Ac15ItemShopCatalog.Disabled;

    private static Ac15ItemShopSeason MapSeason(BlueItemShopSeason season) => new()
    {
        SeasonId = season.SeasonId,
        VerupNo = season.VerupNo,
        Telop = season.Telop,
        StartDatetime = season.StartDatetime,
        EndDatetime = season.EndDatetime,
        AfterstartDays = season.AfterstartDays,
        BeforecloseDays = season.BeforecloseDays,
        Items = season.Items.Select(item => new Ac15ItemShopEntry
        {
            ItemNo = item.ItemNo,
            ItemType = item.ItemType,
            ItemId = item.ItemId,
            Price = item.Price
        }).ToArray()
    };

    private static Ac15ItemShopSeason MapSeason(GreenItemShopSeason season) => new()
    {
        SeasonId = season.SeasonId,
        VerupNo = season.VerupNo,
        Telop = season.Telop,
        StartDatetime = season.StartDatetime,
        EndDatetime = season.EndDatetime,
        AfterstartDays = season.AfterstartDays,
        BeforecloseDays = season.BeforecloseDays,
        Items = season.Items.Select(item => new Ac15ItemShopEntry
        {
            ItemNo = item.ItemNo,
            ItemType = item.ItemType,
            ItemId = item.ItemId,
            Price = item.Price
        }).ToArray()
    };

    private static Ac15ItemShopSeason MapSeason(YellowItemShopSeason season) => new()
    {
        SeasonId = season.SeasonId,
        VerupNo = season.VerupNo,
        Telop = season.Telop,
        StartDatetime = season.StartDatetime,
        EndDatetime = season.EndDatetime,
        AfterstartDays = season.AfterstartDays,
        BeforecloseDays = season.BeforecloseDays,
        Items = season.Items.Select(item => new Ac15ItemShopEntry
        {
            ItemNo = item.ItemNo,
            ItemType = item.ItemType,
            ItemId = item.ItemId,
            Price = item.Price
        }).ToArray()
    };

    private static Ac15TaikojukuEntry MapTaikojuku(BlueTaikojukuEntry entry) => new()
    {
        UniqueId = entry.UniqueId,
        DanLevel = entry.DanLevel,
        ChallengeLevel = entry.ChallengeLevel,
        Name = entry.Name,
        Difficulty = entry.Difficulty,
        VerupNo = entry.VerupNo,
        Songs = entry.Songs.Select(song => new Ac15TaikojukuSong
        {
            MusicId = song.MusicId,
            SongNo = song.SongNo,
            Level = song.Level,
            Notes = song.Notes
        }).ToArray()
    };

    private static Ac15TaikojukuEntry MapTaikojuku(GreenTaikojukuEntry entry) => new()
    {
        UniqueId = entry.UniqueId,
        DanLevel = entry.DanLevel,
        ChallengeLevel = entry.ChallengeLevel,
        Name = entry.Name,
        Difficulty = entry.Difficulty,
        VerupNo = entry.VerupNo,
        Songs = entry.Songs.Select(song => new Ac15TaikojukuSong
        {
            MusicId = song.MusicId,
            SongNo = song.SongNo,
            Level = song.Level,
            Notes = song.Notes
        }).ToArray()
    };

    private static Ac15TaikojukuEntry MapTaikojuku(YellowTaikojukuEntry entry) => new()
    {
        UniqueId = entry.UniqueId,
        DanLevel = entry.DanLevel,
        ChallengeLevel = entry.ChallengeLevel,
        Name = entry.Name,
        Difficulty = entry.Difficulty,
        VerupNo = entry.VerupNo,
        Songs = entry.Songs.Select(song => new Ac15TaikojukuSong
        {
            MusicId = song.MusicId,
            SongNo = song.SongNo,
            Level = song.Level,
            Notes = song.Notes
        }).ToArray()
    };
}
