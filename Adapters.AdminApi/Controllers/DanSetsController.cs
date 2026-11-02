namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>
/// Server Management › Dani Dojo: the course sets. A set's courses are read and saved as a data file
/// (api/GameData/Files/{file}); this manages the sets themselves. Admin only.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
[ApiScope(ApiScopes.AdminServer, ApiScopes.AdminServer)]
public class DanSetsController(IGameDataCatalog catalog, ITaikoDbContext context) : BaseAdminController<DanSetsController>
{
    public record CreateRequest(string Name, string CopyFrom);
    public record RenameRequest(string Name);

    [HttpGet]
    public IReadOnlyList<DanSetInfo> Get() => catalog.Nijiiro().GetDanSets();

    [HttpPost]
    public async Task<IActionResult> Create(CreateRequest request)
    {
        if (!ValidName(request.Name)) return BadRequest(new { message = "A set needs a name (up to 40 characters)." });
        var id = await catalog.Nijiiro().CreateDanSetAsync(request.Name, request.CopyFrom);
        await Log("danset.create", $"{request.Name} (copy of {request.CopyFrom})");
        return Ok(catalog.Nijiiro().GetDanSets().First(s => s.Id == id));
    }

    [HttpPut("{id}/Name")]
    public async Task<IActionResult> Rename(string id, RenameRequest request)
    {
        if (!ValidName(request.Name)) return BadRequest(new { message = "A set needs a name (up to 40 characters)." });
        if (!await catalog.Nijiiro().RenameDanSetAsync(id, request.Name)) return NotFound();
        await Log("danset.rename", $"{id} → {request.Name}");
        return NoContent();
    }

    [HttpPut("Default/{id}")]
    public async Task<IActionResult> SetDefault(string id)
    {
        if (!await catalog.Nijiiro().SetDefaultDanSetAsync(id)) return NotFound();
        await Log("danset.default", id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!await catalog.Nijiiro().DeleteDanSetAsync(id)) return Conflict(new { message = "The built-in set and the default set can't be deleted." });
        await Log("danset.delete", id);
        return NoContent();
    }

    private static bool ValidName(string? name) => name?.Trim() is { Length: > 0 and <= 40 };

    private async Task Log(string action, string detail)
    {
        this.LogActivity(context, action, null, detail);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
    }
}
