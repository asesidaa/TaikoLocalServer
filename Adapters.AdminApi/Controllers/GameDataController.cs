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
            pair => new MusicDetail
            {
                SongId = pair.Value.SongNo,
                Index = pair.Value.FileOrder,
                SongName = string.IsNullOrWhiteSpace(pair.Value.Title) ? pair.Value.MusicId : pair.Value.Title,
                SongNameEN = string.IsNullOrWhiteSpace(pair.Value.Title) ? pair.Value.MusicId : pair.Value.Title,
                SongNameCN = string.IsNullOrWhiteSpace(pair.Value.Title) ? pair.Value.MusicId : pair.Value.Title,
                SongNameKO = string.IsNullOrWhiteSpace(pair.Value.Title) ? pair.Value.MusicId : pair.Value.Title,
                Genre = MapGreenGenre(pair.Value.CategoryId),
                StarEasy = (int)pair.Value.StarEasy,
                StarNormal = (int)pair.Value.StarNormal,
                StarHard = (int)pair.Value.StarHard,
                StarOni = (int)pair.Value.StarOni,
                StarUra = (int)pair.Value.StarUra
            });
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
            OdaiBorderList = []
        }).ToList();
    }

    private static uint ToWebUiDifficultyLevel(uint greenCourseLevel)
        => greenCourseLevel <= 4 ? greenCourseLevel + 1 : 0;

    private static SongGenre MapGreenGenre(uint categoryId)
    {
        return Enum.IsDefined(typeof(SongGenre), (int)categoryId)
            ? (SongGenre)categoryId
            : SongGenre.Pop;
    }
}
