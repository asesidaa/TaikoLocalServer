using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Application.Catalog.Yellow;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15CatalogSnapshotFactory
{
    public static Ac15CatalogSnapshot FromBlue(IBlueCatalog blue) => new(
        blue.SongHashVersion,
        blue.MusicInfoFileOrder.Select(song => song.SongNo).ToArray(),
        blue.EventFolders,
        blue.Telops.ToDictionary(pair => pair.Key, pair => MapTelop(pair.Value)),
        blue.Recommend.RecommendSong,
        blue.Recommend.RecommendBestSongs.ToArray(),
        MapItemShop(blue.ItemShopCatalog),
        blue.TaikojukuFileOrder.Select(MapTaikojuku).ToArray());

    public static Ac15CatalogSnapshot FromGreen(IGreenCatalog green) => new(
        green.SongHashVersion,
        green.MusicInfoFileOrder.Select(song => song.SongNo).ToArray(),
        green.EventFolders,
        green.Telops.ToDictionary(pair => pair.Key, pair => MapTelop(pair.Value)),
        green.Recommend.RecommendSong,
        green.Recommend.RecommendBestSongs.ToArray(),
        MapItemShop(green.ItemShopCatalog),
        green.TaikojukuFileOrder.Select(MapTaikojuku).ToArray());

    public static Ac15CatalogSnapshot FromYellow(IYellowCatalog yellow) => new(
        yellow.SongHashVersion,
        yellow.MusicInfoFileOrder.Select(song => song.SongNo).ToArray(),
        yellow.EventFolders,
        yellow.Telops.ToDictionary(pair => pair.Key, pair => MapTelop(pair.Value)),
        yellow.Recommend.RecommendSong,
        yellow.Recommend.RecommendBestSongs.ToArray(),
        MapItemShop(yellow.ItemShopCatalog),
        yellow.TaikojukuFileOrder.Select(MapTaikojuku).ToArray());

    private static Ac15TelopEntry MapTelop(BlueTelopEntry entry) => new()
    {
        TelopId = entry.TelopId,
        VerupNo = entry.VerupNo,
        StartDatetime = entry.StartDatetime,
        EndDatetime = entry.EndDatetime,
        Message = entry.Message
    };

    private static Ac15TelopEntry MapTelop(GreenTelopEntry entry) => new()
    {
        TelopId = entry.TelopId,
        VerupNo = entry.VerupNo,
        StartDatetime = entry.StartDatetime,
        EndDatetime = entry.EndDatetime,
        Message = entry.Message
    };

    private static Ac15TelopEntry MapTelop(YellowTelopEntry entry) => new()
    {
        TelopId = entry.TelopId,
        VerupNo = entry.VerupNo,
        StartDatetime = entry.StartDatetime,
        EndDatetime = entry.EndDatetime,
        Message = entry.Message
    };

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
