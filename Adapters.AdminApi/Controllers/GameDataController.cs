using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiScope(ApiScopes.ReadScores, ApiScopes.AdminServer)]
public class GameDataController(IGameDataCatalog catalog, ITaikoDbContext context) : BaseAdminController<GameDataController>
{
    private const int MaxFileBytes = 4 * 1024 * 1024;

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
            GameEra.Kimidori => Ok(BuildKimidoriMusicDetails()),
            GameEra.Momoiro => Ok(BuildMomoiroMusicDetails()),
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
            GameEra.Kimidori => Ok(BuildKimidoriDanData()),
            GameEra.Momoiro => Ok(BuildMomoiroDanData()),
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

    // ---- Nijiiro's data files (Server Management › Game data) ----

    [HttpGet("Files")]
    [ApiScope(ApiScopes.AdminServer, ApiScopes.AdminServer)]
    [Authorize(Policy = AuthPolicies.Admin)]
    public IReadOnlyList<GameDataFileStatus> GetFiles() => catalog.Nijiiro().GetFileStatus();

    /// <summary>Reads every file again (after editing them by hand, or running extract).</summary>
    [HttpPost("Reload")]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<IReadOnlyList<GameDataFileStatus>> Reload()
    {
        await catalog.Nijiiro().ReloadAsync();
        this.LogActivity(context, "gamedata.reload", null);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return catalog.Nijiiro().GetFileStatus();
    }

    [HttpGet("Files/{**name}")]
    [ApiScope(ApiScopes.AdminServer, ApiScopes.AdminServer)]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<IActionResult> GetFile(string name) =>
        await catalog.Nijiiro().ReadDataFileAsync(name) is { } json ? Content(json, "application/json") : NotFound();

    /// <summary>Saves an editable data file (the body is its JSON) and reloads. 400 with the reason when it doesn't parse.</summary>
    [HttpPut("Files/{**name}")]
    [Authorize(Policy = AuthPolicies.Admin)]
    [RequestSizeLimit(MaxFileBytes)]
    [Consumes("application/json")]
    public async Task<IActionResult> PutFile(string name)
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync(HttpContext.RequestAborted);
        if (await catalog.Nijiiro().SaveDataFileAsync(name, json) is { } error)
            return BadRequest(new { message = error });

        this.LogActivity(context, "gamedata.save", null, name);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    // ---- Each game's data files (Game data's Game menu); Nijiiro's are the ones above ----

    [HttpGet("/api/{era}/[controller]/Files")]
    [ApiScope(ApiScopes.AdminServer, ApiScopes.AdminServer)]
    [Authorize(Policy = AuthPolicies.Admin)]
    public IActionResult GetEraFiles(string era)
    {
        if (!TryEnabledEra(era, out var gameEra, out var refused)) return refused;
        return Ok(gameEra == GameEra.Nijiiro ? GetFiles() : EraDataFiles.List(catalog, gameEra));
    }

    [HttpPost("/api/{era}/[controller]/Reload")]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<IActionResult> ReloadEra(string era)
    {
        if (!TryEnabledEra(era, out var gameEra, out var refused)) return refused;
        if (gameEra == GameEra.Nijiiro) return Ok(await Reload());
        await catalog.ReloadAsync(gameEra, HttpContext.RequestAborted);
        this.LogActivity(context, "gamedata.reload", null, gameEra.ToString());
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return Ok(EraDataFiles.List(catalog, gameEra));
    }

    [HttpGet("/api/{era}/[controller]/Files/{name}")]
    [ApiScope(ApiScopes.AdminServer, ApiScopes.AdminServer)]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<IActionResult> GetEraFile(string era, string name)
    {
        if (!TryEnabledEra(era, out var gameEra, out var refused)) return refused;
        if (gameEra == GameEra.Nijiiro) return await GetFile(name);
        return await EraDataFiles.ReadAsync(gameEra, name) is { } json ? Content(json, "application/json") : NotFound();
    }

    /// <summary>Saves one of an older game's files and reloads that game. 400 with the reason when it doesn't load.</summary>
    [HttpPut("/api/{era}/[controller]/Files/{name}")]
    [Authorize(Policy = AuthPolicies.Admin)]
    [RequestSizeLimit(MaxFileBytes)]
    [Consumes("application/json")]
    public async Task<IActionResult> PutEraFile(string era, string name)
    {
        if (!TryEnabledEra(era, out var gameEra, out var refused)) return refused;
        if (gameEra == GameEra.Nijiiro) return await PutFile(name);
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync(HttpContext.RequestAborted);
        if (await EraDataFiles.SaveAsync(catalog, gameEra, name, json, HttpContext.RequestAborted) is { } error)
            return BadRequest(new { message = error });

        this.LogActivity(context, "gamedata.save", null, $"{gameEra}: {name}");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private bool TryEnabledEra(string era, out GameEra gameEra, out IActionResult refused)
    {
        refused = null!;
        if (!EraRoute.TryParse(era, out gameEra))
        {
            refused = EraRoute.BadEra(era);
            return false;
        }
        if (!catalog.LoadResults.ContainsKey(gameEra))
        {
            refused = BadRequest(new { message = $"{gameEra} is off in ServerSettings.json." });
            return false;
        }
        return true;
    }

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

    private Dictionary<uint, MusicDetail> BuildKimidoriMusicDetails()
    {
        return catalog.Kimidori().KimidoriMusicInfos.ToDictionary(
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

    private Dictionary<uint, MusicDetail> BuildMomoiroMusicDetails()
    {
        return catalog.Momoiro().MomoiroMusicInfos.ToDictionary(
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

    private List<DanData> BuildKimidoriDanData()
    {
        return catalog.Kimidori().DaniFileOrder.Select(entry => new DanData
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

    private List<DanData> BuildMomoiroDanData()
    {
        return catalog.Momoiro().DaniFileOrder.Select(entry => new DanData
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

    private static uint ToWebUiDifficultyLevel(Difficulty difficulty)
        => Ac15Difficulty.ToProtocol(difficulty);

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
