namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GameDataController(IGameDataCatalog catalog, ITaikoDbContext context) : BaseAdminController<GameDataController>
{
    private const int MaxFileBytes = 4 * 1024 * 1024;

    [HttpGet("DanData")]
    public IActionResult GetDanData() => Ok(catalog.GetCommonDanDataDictionary().Values);

    [HttpGet("MusicDetails")]
    public IActionResult GetMusicDetails() => Ok(catalog.GetMusicDetailDictionary());

    [HttpGet("Costumes")]
    public IActionResult GetCostumes() => Ok(catalog.GetCostumeList());

    [HttpGet("Titles")]
    public IActionResult GetTitles() => Ok(catalog.GetTitleDictionary());

    [HttpGet("LockedCostumes")]
    public IActionResult GetLockedCostumes() => Ok(catalog.GetLockedCostumeDataDictionary());

    [HttpGet("LockedTitles")]
    public IActionResult GetLockedTitles() => Ok(catalog.GetLockedTitleDataDictionary());

    // ---- Server Management › Game data (admin) ----

    /// <summary>Every data file and how it last loaded.</summary>
    [HttpGet("Files")]
    [Authorize(Policy = AuthPolicies.Admin)]
    public IReadOnlyList<GameDataFileStatus> GetFiles() => catalog.GetFileStatus();

    /// <summary>Reads every file again (after editing them by hand, or running extract).</summary>
    [HttpPost("Reload")]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<IReadOnlyList<GameDataFileStatus>> Reload()
    {
        await catalog.ReloadAsync();
        this.LogActivity(context, "gamedata.reload", null);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return catalog.GetFileStatus();
    }

    [HttpGet("Files/{**name}")]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<IActionResult> GetFile(string name) =>
        await catalog.ReadDataFileAsync(name) is { } json ? Content(json, "application/json") : NotFound();

    /// <summary>Saves an editable data file (the body is its JSON) and reloads. 400 with the reason when it doesn't parse.</summary>
    [HttpPut("Files/{**name}")]
    [Authorize(Policy = AuthPolicies.Admin)]
    [RequestSizeLimit(MaxFileBytes)]
    [Consumes("application/json")]
    public async Task<IActionResult> PutFile(string name)
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync(HttpContext.RequestAborted);
        if (await catalog.SaveDataFileAsync(name, json) is { } error)
            return BadRequest(new { message = error });

        this.LogActivity(context, "gamedata.save", null, name);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }
}
