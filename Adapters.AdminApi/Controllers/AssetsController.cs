using Microsoft.AspNetCore.StaticFiles;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>
/// Serves the don-chan preview assets written by <c>tools/extract</c> to logged-in users only.
/// They live in the exe's <c>wwwroot/don-model</c>, which the static-file middleware deliberately does not serve.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssetsController : BaseAdminController<AssetsController>
{
    private static readonly string Root = Path.GetFullPath(Path.Combine(PathHelper.GetRootPath(), "don-model"));

    // Only what extract writes; anything else 404s.
    private static readonly FileExtensionContentTypeProvider ContentTypes = new(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [".glb"] = "model/gltf-binary",
        [".png"] = "image/png",
        [".json"] = "application/json",
    });

    [HttpGet("{**path}")]
    public IActionResult Get(string path)
    {
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
