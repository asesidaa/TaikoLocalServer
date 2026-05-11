namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoriteSongsController(ITaikoDbContext context) : BaseAdminController<FavoriteSongsController>
{
    [HttpPost]
    public async Task<IActionResult> UpdateFavoriteSong(SetFavoriteRequest request)
    {
        if (this.AuthorizeOwnerOrAdmin(request.Baid) is { } forbid)
            return forbid;

        var user = await context.UserData.FindAsync(request.Baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await context.GetOrCreateNijiiroSaveDataAsync(request.Baid, HttpContext.RequestAborted);
        var favoriteSet = new HashSet<uint>(saveData.FavoriteSongsArray);
        if (request.IsFavorite)
        {
            favoriteSet.Add(request.SongId);
        }
        else
        {
            favoriteSet.Remove(request.SongId);
        }

        saveData.FavoriteSongsArray = favoriteSet.ToList();
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpGet("{baid}")]
    public async Task<IActionResult> GetFavoriteSongs(uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await context.GetOrCreateNijiiroSaveDataAsync(baid, HttpContext.RequestAborted);
        return Ok(saveData.FavoriteSongsArray);
    }
}
