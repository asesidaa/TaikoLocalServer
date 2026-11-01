using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>
/// Serves the game assets written by <c>tools/extract</c> to the WebUI's own pages, for logged-in users only.
/// They live in the exe's <c>wwwroot/don-model</c>, which the static-file middleware deliberately does not serve.
/// A loose measure, not DRM: the WebUI holds them as blobs, and anything a browser renders can be dumped.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting(DependencyInjection.AssetsRateLimitPolicy)]
public class AssetsController : BaseAdminController<AssetsController>
{
    private static readonly string Root = Path.GetFullPath(Path.Combine(PathHelper.GetRootPath(), "don-model"));

    // Only what extract writes; anything else 404s.
    private static readonly FileExtensionContentTypeProvider ContentTypes = new(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [".glb"] = "model/gltf-binary",
        [".png"] = "image/png",
        [".json"] = "application/json",
        [".wav"] = "audio/wav",
    });

    [HttpGet("{**path}")]
    public IActionResult Get(string path)
    {
        // Only fetch() from this site's own pages (the WebUI and its /don-embed/): opening the URL, an <img>
        // hotlink or another site gets nothing. Browsers set these headers themselves; it also covers local
        // mode (AuthenticationRequired off), where [Authorize] lets everyone through.
        if (Request.Headers["Sec-Fetch-Site"] != "same-origin" || Request.Headers["Sec-Fetch-Dest"] != "empty")
        {
            return NotFound();
        }

        var fullPath = Path.GetFullPath(Path.Combine(Root, path));
        // Rejects "..", rooted paths and anything outside don-model.
        if (!fullPath.StartsWith(Root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || !System.IO.File.Exists(fullPath)
            || !ContentTypes.TryGetContentType(fullPath, out var contentType))
        {
            return NotFound();
        }

        Response.Headers.CacheControl = "private, max-age=86400";
        return PhysicalFile(fullPath, contentType);
    }
}
