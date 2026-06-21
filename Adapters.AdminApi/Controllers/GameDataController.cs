using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GameDataController(IGameDataCatalog catalog) : BaseAdminController<GameDataController>
{
    [HttpGet("MusicDetails")]
    public IActionResult GetMusicDetails() => GetMusicDetails(nameof(GameEra.Nijiiro));

    [HttpGet("/api/{era}/[controller]/MusicDetails")]
    public IActionResult GetMusicDetails(string era)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
            return EraRoute.BadEra(era);

        return gameEra switch
        {
            GameEra.Nijiiro => Ok(catalog.Nijiiro().GetMusicDetailDictionary()),
            GameEra.Green => Ok(BuildGreenMusicDetails()),
            GameEra.Blue => Ok(BuildBlueMusicDetails()),
            GameEra.Yellow => Ok(BuildYellowMusicDetails()),
            GameEra.Red => Ok(BuildRedMusicDetails()),
            GameEra.White => Ok(BuildWhiteMusicDetails()),
            GameEra.Murasaki => Ok(BuildMurasakiMusicDetails()),
            _ => EraRoute.BadEra(era)
        };
    }

    [HttpGet("DanData")]
    public IActionResult GetDanData() => GetDanData(nameof(GameEra.Nijiiro));

    [HttpGet("/api/{era}/[controller]/DanData")]
    public IActionResult GetDanData(string era)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
            return EraRoute.BadEra(era);

        return gameEra switch
        {
            GameEra.Nijiiro => Ok(catalog.Nijiiro().GetCommonDanDataDictionary().Values.ToList()),
            GameEra.Green => Ok(BuildGreenDanData()),
            GameEra.Blue => Ok(BuildBlueDanData()),
            GameEra.Yellow => Ok(BuildYellowDanData()),
            GameEra.Red => Ok(BuildRedDanData()),
            GameEra.White => Ok(BuildWhiteDanData()),
            GameEra.Murasaki => Ok(BuildMurasakiDanData()),
            _ => EraRoute.BadEra(era)
        };
    }

    [HttpGet("Costumes")]
    public IActionResult GetCostumes() => Ok(catalog.Nijiiro().GetCostumeList());

    [HttpGet("Titles")]
    public IActionResult GetTitles() => Ok(catalog.Nijiiro().GetTitleDictionary());

    [HttpGet("LockedCostumes")]
    public IActionResult GetLockedCostumes() => Ok(catalog.Nijiiro().GetLockedCostumeDataDictionary());

    [HttpGet("LockedTitles")]
    public IActionResult GetLockedTitles() => Ok(catalog.Nijiiro().GetLockedTitleDataDictionary());

    private Dictionary<uint, MusicDetail> BuildGreenMusicDetails()
    {
        return catalog.Green().GreenMusicInfos.ToDictionary(
            pair => pair.Key,
            pair => BuildAc15MusicDetail(
                pair.Value.SongNo,
                pair.Value.FileOrder,
                pair.Value.Title,
                pair.Value.MusicId,
                pair.Value.CategoryId,
                (int)pair.Value.StarEasy,
                (int)pair.Value.StarNormal,
                (int)pair.Value.StarHard,
                (int)pair.Value.StarOni,
                (int)pair.Value.StarUra));
    }

    private List<DanData> BuildGreenDanData()
    {
        return catalog.Green().TaikojukuFileOrder.Select(entry => new DanData
        {
            DanId = entry.ChallengeLevel,
            Title = string.IsNullOrWhiteSpace(entry.Name) ? entry.UniqueId.ToString() : entry.Name,
            VerupNo = entry.VerupNo,
            OdaiSongList = entry.Songs.Select(song => new DanData.OdaiSong
            {
                SongNo = song.SongNo,
                Level = ToWebUiDifficultyLevel(song.Level)
            }).ToList(),
            OdaiBorderList = BuildAc15OdaiBorders(entry)
        }).ToList();
    }

    private Dictionary<uint, MusicDetail> BuildBlueMusicDetails()
    {
        return catalog.Blue().BlueMusicInfos.ToDictionary(
            pair => pair.Key,
            pair => BuildAc15MusicDetail(
                pair.Value.SongNo,
                pair.Value.FileOrder,
                pair.Value.Title,
                pair.Value.MusicId,
                pair.Value.CategoryId,
                (int)pair.Value.StarEasy,
                (int)pair.Value.StarNormal,
                (int)pair.Value.StarHard,
                (int)pair.Value.StarOni,
                (int)pair.Value.StarUra));
    }

    private List<DanData> BuildBlueDanData()
    {
        return catalog.Blue().TaikojukuFileOrder.Select(entry => new DanData
        {
            DanId = entry.ChallengeLevel,
            Title = string.IsNullOrWhiteSpace(entry.Name) ? entry.UniqueId.ToString() : entry.Name,
            VerupNo = entry.VerupNo,
            OdaiSongList = entry.Songs.Select(song => new DanData.OdaiSong
            {
                SongNo = song.SongNo,
                Level = ToWebUiDifficultyLevel(song.Level)
            }).ToList(),
            OdaiBorderList = BuildAc15OdaiBorders(entry)
        }).ToList();
    }

    private Dictionary<uint, MusicDetail> BuildYellowMusicDetails()
    {
        return catalog.Yellow().YellowMusicInfos.ToDictionary(
            pair => pair.Key,
            pair => BuildAc15MusicDetail(
                pair.Value.SongNo,
                pair.Value.FileOrder,
                pair.Value.Title,
                pair.Value.MusicId,
                pair.Value.CategoryId,
                (int)pair.Value.StarEasy,
                (int)pair.Value.StarNormal,
                (int)pair.Value.StarHard,
                (int)pair.Value.StarOni,
                (int)pair.Value.StarUra));
    }

    private Dictionary<uint, MusicDetail> BuildRedMusicDetails()
    {
        return catalog.Red().RedMusicInfos.ToDictionary(
            pair => pair.Key,
            pair => BuildAc15MusicDetail(
                pair.Value.SongNo,
                pair.Value.FileOrder,
                pair.Value.Title,
                pair.Value.MusicId,
                pair.Value.CategoryId,
                pair.Value.StarEasy,
                pair.Value.StarNormal,
                pair.Value.StarHard,
                pair.Value.StarOni,
                pair.Value.StarUra));
    }

    private Dictionary<uint, MusicDetail> BuildWhiteMusicDetails()
    {
        return catalog.White().WhiteMusicInfos.ToDictionary(
            pair => pair.Key,
            pair => BuildAc15MusicDetail(
                pair.Value.SongNo,
                pair.Value.FileOrder,
                pair.Value.Title,
                pair.Value.MusicId,
                pair.Value.CategoryId,
                pair.Value.StarEasy,
                pair.Value.StarNormal,
                pair.Value.StarHard,
                pair.Value.StarOni,
                pair.Value.StarUra));
    }

    private Dictionary<uint, MusicDetail> BuildMurasakiMusicDetails()
    {
        return catalog.Murasaki().MurasakiMusicInfos.ToDictionary(
            pair => pair.Key,
            pair => BuildAc15MusicDetail(
                pair.Value.SongNo,
                pair.Value.FileOrder,
                pair.Value.Title,
                pair.Value.MusicId,
                pair.Value.CategoryId,
                pair.Value.StarEasy,
                pair.Value.StarNormal,
                pair.Value.StarHard,
                pair.Value.StarOni,
                pair.Value.StarUra));
    }

    private List<DanData> BuildYellowDanData()
    {
        return catalog.Yellow().TaikojukuFileOrder.Select(entry => new DanData
        {
            DanId = entry.ChallengeLevel,
            Title = string.IsNullOrWhiteSpace(entry.Name) ? entry.UniqueId.ToString() : entry.Name,
            VerupNo = entry.VerupNo,
            OdaiSongList = entry.Songs.Select(song => new DanData.OdaiSong
            {
                SongNo = song.SongNo,
                Level = ToWebUiDifficultyLevel(song.Level)
            }).ToList(),
            OdaiBorderList = BuildAc15OdaiBorders(entry)
        }).ToList();
    }

    private List<DanData> BuildRedDanData()
    {
        return catalog.Red().TaikojukuFileOrder.Select(entry => new DanData
        {
            DanId = entry.ChallengeLevel,
            Title = string.IsNullOrWhiteSpace(entry.Name) ? entry.UniqueId.ToString() : entry.Name,
            VerupNo = entry.VerupNo,
            OdaiSongList = entry.Songs.Select(song => new DanData.OdaiSong
            {
                SongNo = song.SongNo,
                Level = ToWebUiDifficultyLevel(song.Level)
            }).ToList(),
            OdaiBorderList = BuildAc15OdaiBorders(entry)
        }).ToList();
    }

    private List<DanData> BuildWhiteDanData()
    {
        return catalog.White().TaikojukuFileOrder.Select(entry => new DanData
        {
            DanId = entry.ChallengeLevel,
            Title = string.IsNullOrWhiteSpace(entry.Name) ? entry.UniqueId.ToString() : entry.Name,
            VerupNo = entry.VerupNo,
            OdaiSongList = entry.Songs.Select(song => new DanData.OdaiSong
            {
                SongNo = song.SongNo,
                Level = ToWebUiDifficultyLevel(song.Level)
            }).ToList(),
            OdaiBorderList = BuildAc15OdaiBorders(entry)
        }).ToList();
    }

    private List<DanData> BuildMurasakiDanData()
    {
        return catalog.Murasaki().TaikojukuFileOrder.Select(entry => new DanData
        {
            DanId = entry.ChallengeLevel,
            Title = string.IsNullOrWhiteSpace(entry.Name) ? entry.UniqueId.ToString() : entry.Name,
            VerupNo = entry.VerupNo,
            OdaiSongList = entry.Songs.Select(song => new DanData.OdaiSong
            {
                SongNo = song.SongNo,
                Level = ToWebUiDifficultyLevel(song.Level)
            }).ToList(),
            OdaiBorderList = BuildAc15OdaiBorders(entry)
        }).ToList();
    }

    private static List<DanData.OdaiBorder> BuildAc15OdaiBorders(Ac15TaikojukuEntry entry)
    {
        var red = entry.Conditions;
        var gold = entry.ExcellentConditions;
        var borders = new List<DanData.OdaiBorder>();

        AddAc15OdaiBorder(borders, DanConditionType.SoulGauge, red.SoulGauge, gold.SoulGauge);
        AddAc15OdaiBorder(borders, DanConditionType.GoodCount, red.GoodCount, gold.GoodCount);
        AddAc15OdaiBorder(borders, DanConditionType.OkCount, red.OkCount, gold.OkCount);
        AddAc15OdaiBorder(borders, DanConditionType.BadCount, red.BadCount, gold.BadCount);
        AddAc15OdaiBorder(borders, DanConditionType.ComboCount, red.ComboCount, gold.ComboCount);
        AddAc15OdaiBorder(borders, DanConditionType.DrumrollCount, red.DrumrollCount, gold.DrumrollCount);
        AddAc15OdaiBorder(borders, DanConditionType.Score, red.Score, gold.Score);
        AddAc15OdaiBorder(borders, DanConditionType.TotalHitCount, red.TotalHitCount, gold.TotalHitCount);

        return borders;
    }

    private static void AddAc15OdaiBorder(
        List<DanData.OdaiBorder> borders,
        DanConditionType type,
        uint redBorder,
        uint goldBorder)
    {
        if (redBorder == 0 && goldBorder == 0)
        {
            return;
        }

        borders.Add(new DanData.OdaiBorder
        {
            OdaiType = (uint)type,
            BorderType = (uint)DanBorderType.All,
            RedBorderTotal = redBorder,
            GoldBorderTotal = goldBorder
        });
    }

    private static uint ToWebUiDifficultyLevel(uint greenCourseLevel)
        => greenCourseLevel <= 4 ? greenCourseLevel + 1 : 0;

    private static MusicDetail BuildAc15MusicDetail(
        uint songNo,
        int fileOrder,
        string title,
        string musicId,
        uint categoryId,
        int starEasy,
        int starNormal,
        int starHard,
        int starOni,
        int starUra)
    {
        var songName = string.IsNullOrWhiteSpace(title) ? musicId : title;
        var nonJapaneseSongName = Ac15MusicMetadata.NormalizeFullWidthAscii(songName);
        return new MusicDetail
        {
            SongId = songNo,
            Index = fileOrder,
            SongName = songName,
            SongNameEN = nonJapaneseSongName,
            SongNameCN = nonJapaneseSongName,
            SongNameKO = nonJapaneseSongName,
            Genre = MapAc15Genre(categoryId),
            StarEasy = starEasy,
            StarNormal = starNormal,
            StarHard = starHard,
            StarOni = starOni,
            StarUra = starUra
        };
    }

    private static SongGenre MapAc15Genre(uint categoryId)
    {
        return Enum.IsDefined(typeof(SongGenre), (int)categoryId)
            ? (SongGenre)categoryId
            : SongGenre.Pop;
    }
}
