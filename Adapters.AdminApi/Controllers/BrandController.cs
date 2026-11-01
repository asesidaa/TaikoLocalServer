using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>
/// The WebUI's logo and favicon: the don note extract cuts from the game (<c>game_data/ui/don_note.png</c>). Unlike
/// api/Assets it's open to everyone, signed in or not, and to &lt;img&gt; and the browser's favicon request, since the
/// sign-in page shows it too. One fixed file; 404 until extract has run, and the WebUI keeps its own mark then.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
[OpenInEveryServerState]
public class BrandController : BaseAdminController<BrandController>
{
    private static readonly string IconPath = Path.Combine(PathHelper.GetRootPath(), PathHelper.GameDataFolder, "ui", "don_note.png");

    [HttpGet("Icon")]
    public IActionResult Icon()
    {
        if (!System.IO.File.Exists(IconPath)) return NotFound();
        Response.Headers.CacheControl = "public, max-age=86400";
        return PhysicalFile(IconPath, "image/png");
    }
}
