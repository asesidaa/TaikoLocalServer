using Microsoft.Extensions.Options;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>
/// The games in ServerSettings.json's Eras and how each one's files loaded (Server Management › Status › Games, setup).
/// Turning a game on or off is config (a restart); Reload re-reads one game's files without one. Admin only.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
[ApiScope(ApiScopes.AdminServer, ApiScopes.AdminServer)]
public class GamesController(IGameDataCatalog catalog, IOptions<ServerSettings> settings, ITaikoDbContext context, IShopLiveSeasons liveSeasons)
    : BaseAdminController<GamesController>
{
    [HttpGet]
    public IReadOnlyList<GameStatus> Get() => Enum.GetValues<GameEra>().Select(Status).ToList();

    /// <summary>Re-reads one game's files, as at start-up (Check again).</summary>
    [HttpPost("{era}/Reload")]
    public async Task<IActionResult> Reload(string era)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
            return EraRoute.BadEra(era);
        if (!catalog.LoadResults.ContainsKey(gameEra))
            return BadRequest(new { message = $"{gameEra} is off in ServerSettings.json." });

        await catalog.ReloadAsync(gameEra, HttpContext.RequestAborted);
        this.LogActivity(context, "gamedata.reload", null, gameEra.ToString());
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return Ok(Status(gameEra));
    }

    private GameStatus Status(GameEra era)
    {
        var eraSettings = settings.Value.Eras.GetValueOrDefault(era.ToString());
        catalog.LoadResults.TryGetValue(era, out var load);
        // Off, or on but never loaded: only the folder is known.
        if (load is null)
        {
            var (offFolder, offFound) = FolderOf(era, eraSettings);
            return new GameStatus(era.ToString(), false, offFolder, offFound, null, null, 0, 0, 0, 0);
        }

        var (folder, found) = FolderOf(era, eraSettings);
        var (costumes, titles, tones) = NameCounts(era);
        int? filesLoaded = null, filesTotal = null;
        if (era == GameEra.Nijiiro)
        {
            var files = catalog.Nijiiro().GetFileStatus();
            filesTotal = files.Count;
            filesLoaded = files.Count(f => !f.Missing && f.Error is null);
        }

        return new GameStatus(era.ToString(), true, folder, found, load.At, load.Error, catalog.For(era).MusicInfos.Count,
            costumes, titles, tones, filesLoaded, filesTotal,
            eraSettings?.EnableShop == true ? liveSeasons.Get(era) ?? eraSettings.ActiveShopSeasonId : null,
            eraSettings?.IsDonChallengeEnabled() == true ? eraSettings.GetActiveDonChallengeBundleId() : null);
    }

    // Nijiiro: the install extract reads (its datatables live in wwwroot/data). The older games: the folder their songs
    // and events are read from, wwwroot/data/<era>/data (Eras.<Game>.GameDataPath is only used to extract their names).
    private static (string Folder, bool Found) FolderOf(GameEra era, EraSettings? eraSettings)
    {
        if (era == GameEra.Nijiiro)
        {
            var install = eraSettings?.GameDataPath?.Trim() ?? "";
            return (install, install.Length > 0 && Directory.Exists(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, install))));
        }

        var path = Path.Combine(PathHelper.GetDataPath(era), "data");
        return (Path.GetRelativePath(AppContext.BaseDirectory, path).Replace('\\', '/'), Directory.Exists(path));
    }

    private (int Costumes, int Titles, int Tones) NameCounts(GameEra era) => era switch
    {
        GameEra.Nijiiro => (catalog.Nijiiro().GetCostumeList().Count, catalog.Nijiiro().GetTitleDictionary().Count, catalog.Nijiiro().GetNeiroDictionary().Count),
        GameEra.Green => (catalog.Green().GetCostumeList().Count, catalog.Green().GetTitleDictionary().Count, catalog.Green().GetNeiroDictionary().Count),
        GameEra.Blue => (catalog.Blue().GetCostumeList().Count, catalog.Blue().GetTitleDictionary().Count, catalog.Blue().GetNeiroDictionary().Count),
        GameEra.Yellow => (catalog.Yellow().GetCostumeList().Count, catalog.Yellow().GetTitleDictionary().Count, catalog.Yellow().GetNeiroDictionary().Count),
        GameEra.Red => (catalog.Red().GetCostumeList().Count, catalog.Red().GetTitleDictionary().Count, catalog.Red().GetNeiroDictionary().Count),
        GameEra.White => (catalog.White().GetCostumeList().Count, catalog.White().GetTitleDictionary().Count, catalog.White().GetNeiroDictionary().Count),
        GameEra.Murasaki => (catalog.Murasaki().GetCostumeList().Count, catalog.Murasaki().GetTitleDictionary().Count, catalog.Murasaki().GetNeiroDictionary().Count),
        GameEra.Kimidori => (catalog.Kimidori().GetCostumeList().Count, catalog.Kimidori().GetTitleDictionary().Count, catalog.Kimidori().GetNeiroDictionary().Count),
        GameEra.Momoiro => (catalog.Momoiro().GetCostumeList().Count, catalog.Momoiro().GetTitleDictionary().Count, catalog.Momoiro().GetNeiroDictionary().Count),
        _ => (0, 0, 0)
    };
}
