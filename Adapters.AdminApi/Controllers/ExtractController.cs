namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>
/// "Run extract": copies the game files in from Eras.Nijiiro.GameDataPath, or from a game folder an admin sends from their browser
/// (AllowRemoteGameDir) (Server Management › Game data, setup). Admin only.
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

    /// <summary>
    /// Starts a run; poll GET for its report. <paramref name="mods"/> layers the install's Data_mods.
    /// <paramref name="uploaded"/> runs on the files sent to Upload instead of Eras.Nijiiro.GameDataPath.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Start([FromQuery] bool mods = true, [FromQuery] bool uploaded = false)
    {
        if (extract.Start(mods, uploaded) is { } error)
            return Conflict(new { message = error });
        this.LogActivity(context, "gamedata.extract", null, string.Join(", ", new[] { mods ? "with mods" : "", uploaded ? "sent from the browser" : "" }.Where(s => s.Length > 0)));
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return Accepted(extract.Status);
    }

    /// <summary>The game files to send: only these are accepted.</summary>
    [HttpGet("Inputs")]
    public async Task<IActionResult> Inputs()
    {
        var (inputs, error) = await extract.InputsAsync(HttpContext.RequestAborted);
        return inputs is null ? Conflict(new { message = error }) : Ok(inputs);
    }

    /// <summary>Starts sending a game folder (drops what was sent before).</summary>
    [HttpDelete("Upload")]
    public IActionResult ClearUpload() =>
        extract.ClearUpload() is { } error ? Conflict(new { message = error }) : NoContent();

    /// <summary>A batch of game files: each form field's name is the file's path in the game folder.</summary>
    [HttpPost("Upload")]
    [RequestSizeLimit(UploadLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = UploadLimit)]
    public async Task<IActionResult> Upload()
    {
        if (!Request.HasFormContentType) return BadRequest(new { message = "Send the files as multipart form data." });
        var form = await Request.ReadFormAsync(HttpContext.RequestAborted);
        foreach (var file in form.Files)
        {
            await using var stream = file.OpenReadStream();
            if (await extract.SaveUploadAsync(file.Name, stream, HttpContext.RequestAborted) is { } error)
                return BadRequest(new { message = error });
        }
        return NoContent();
    }
}
