namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>"Run extract": copies the game files in from GamePath (Server Management › Game data, setup). Admin only.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
public class ExtractController(IExtractRunner extract, ITaikoDbContext context) : BaseAdminController<ExtractController>
{
    [HttpGet]
    public ExtractStatus Get() => extract.Status;

    /// <summary>Starts a run; poll GET for its report. <paramref name="mods"/> layers the install's Data_mods.</summary>
    [HttpPost]
    public async Task<IActionResult> Start([FromQuery] bool mods = true)
    {
        if (extract.Start(mods) is { } error)
            return Conflict(new { message = error });
        this.LogActivity(context, "gamedata.extract", null, mods ? "with mods" : "");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return Accepted(extract.Status);
    }
}
