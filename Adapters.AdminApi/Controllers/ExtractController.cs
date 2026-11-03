namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>
/// Extract: copies games' files in from their installs on the server (Eras.Nijiiro.GameDataPath, Eras.&lt;Game&gt;.GamePath),
/// or from game folders an admin sends from their browser (AllowRemoteGameDir), one game after another (Server
/// Management › Games, Game data, setup). Admin only.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
[ApiScope(ApiScopes.AdminServer, ApiScopes.AdminServer)]
public class ExtractController(IExtractRunner extract, ITaikoDbContext context) : BaseAdminController<ExtractController>
{
    /// <summary>The largest batch of sent files; the browser keeps under it (and sends a bigger file alone).</summary>
    private const long UploadLimit = 128L * 1024 * 1024;

    [HttpGet]
    public ExtractStatus Get() => extract.Status;

    /// <summary>Starts a run of the games asked for, one after another; poll GET for its progress.</summary>
    [HttpPost]
    public async Task<IActionResult> Start([FromBody] ExtractRequest request)
    {
        var games = new List<GameEra>();
        foreach (var name in request.Games)
        {
            if (!EraRoute.TryParse(name, out var game)) return EraRoute.BadEra(name);
            games.Add(game);
        }
        var uploaded = new HashSet<GameEra>();
        foreach (var name in request.Uploaded ?? [])
        {
            if (!EraRoute.TryParse(name, out var game)) return EraRoute.BadEra(name);
            uploaded.Add(game);
        }

        if (extract.Start(games, request.Mods, uploaded, request.DatatablesOnly) is { } error)
            return Conflict(new { message = error });
        var detail = string.Join(", ", games.Select(g => uploaded.Contains(g) ? $"{g} (sent from the browser)" : g.ToString()));
        if (games.Contains(GameEra.Nijiiro))
        {
            if (request.Mods) detail += ", with mods";
            if (request.DatatablesOnly) detail += ", Nijiiro datatables only";
        }
        this.LogActivity(context, "gamedata.extract", null, detail);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return Accepted(extract.Status);
    }

    /// <summary>Stops the run at the current game's next file; the games after it don't start.</summary>
    [HttpPost("Stop")]
    public async Task<IActionResult> Stop()
    {
        if (!extract.Stop()) return Conflict(new { message = "Nothing is extracting." });
        this.LogActivity(context, "gamedata.extract.stop", null, extract.Status.Run?.Current ?? "");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return Ok(extract.Status);
    }

    /// <summary>The folders on the server, to pick a game's install (null <paramref name="path"/>: the drives).</summary>
    [HttpGet("Folders")]
    public IActionResult Folders([FromQuery] string game = nameof(GameEra.Nijiiro), [FromQuery] string? path = null)
    {
        if (!EraRoute.TryParse(game, out var era)) return EraRoute.BadEra(game);
        return Ok(extract.Browse(era, path));
    }

    /// <summary>Reads a game from this folder on the server from now on (saved in ServerSettings.json).</summary>
    [HttpPut("Path")]
    public async Task<IActionResult> SetPath([FromBody] ExtractPathRequest request)
    {
        if (!EraRoute.TryParse(request.Game, out var era)) return EraRoute.BadEra(request.Game);
        if (await extract.SetGamePathAsync(era, request.Path, HttpContext.RequestAborted) is { } error)
            return BadRequest(new { message = error });
        this.LogActivity(context, "gamedata.path", null, $"{era}: {request.Path}");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return Ok(extract.Status);
    }

    /// <summary>The game files to send for a game: only these are accepted.</summary>
    [HttpGet("Inputs")]
    public async Task<IActionResult> Inputs([FromQuery] string game = nameof(GameEra.Nijiiro))
    {
        if (!EraRoute.TryParse(game, out var era)) return EraRoute.BadEra(game);
        var (inputs, error) = await extract.InputsAsync(era, HttpContext.RequestAborted);
        return inputs is null ? Conflict(new { message = error }) : Ok(inputs);
    }

    /// <summary>Starts sending a game's folder (drops what was sent for it before).</summary>
    [HttpDelete("Upload")]
    public IActionResult ClearUpload([FromQuery] string game = nameof(GameEra.Nijiiro))
    {
        if (!EraRoute.TryParse(game, out var era)) return EraRoute.BadEra(game);
        return extract.ClearUpload(era) is { } error ? Conflict(new { message = error }) : NoContent();
    }

    /// <summary>A batch of a game's files: each form field's name is the file's path in the game folder.</summary>
    [HttpPost("Upload")]
    [RequestSizeLimit(UploadLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = UploadLimit)]
    public async Task<IActionResult> Upload([FromQuery] string game = nameof(GameEra.Nijiiro))
    {
        if (!EraRoute.TryParse(game, out var era)) return EraRoute.BadEra(game);
        if (!Request.HasFormContentType) return BadRequest(new { message = "Send the files as multipart form data." });
        var form = await Request.ReadFormAsync(HttpContext.RequestAborted);
        foreach (var file in form.Files)
        {
            await using var stream = file.OpenReadStream();
            if (await extract.SaveUploadAsync(era, file.Name, stream, HttpContext.RequestAborted) is { } error)
                return BadRequest(new { message = error });
        }
        return NoContent();
    }
}
