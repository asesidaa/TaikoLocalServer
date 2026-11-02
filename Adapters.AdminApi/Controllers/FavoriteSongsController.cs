namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>
/// A player's favourite songs, in order: the cabinet's Favourites folder lists them as the array is stored
/// (UserDatum.FavoriteSongsArray, sent as AryFavoriteSongNoes). The hearts add or remove one; the Favourites page
/// saves the whole ordered list.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiScope(ApiScopes.ReadScores, ApiScopes.AdminUsers)]
public class FavoriteSongsController(ITaikoDbContext context, IGameDataCatalog catalog) : BaseAdminController<FavoriteSongsController>
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

        // Keeps the order: a new one goes last, a removed one leaves the rest where they were.
        var favorites = user.FavoriteSongsArray.ToList();
        if (request.IsFavorite && !favorites.Contains(request.SongId))
        {
            favorites.Add(request.SongId);
        }
        else if (!request.IsFavorite)
        {
            favorites.Remove(request.SongId);
        }

        user.FavoriteSongsArray = favorites;
        this.LogIfSomeoneElse(context, "favourites.edit", request.Baid, $"#{request.SongId} {(request.IsFavorite ? "added" : "removed")}");
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

        return Ok(user.FavoriteSongsArray);
    }

    /// <summary>The whole list, in the order the Favourites folder should show it (the Favourites page's Save).</summary>
    // shortcut: no cap on the count; the game may have one (not found in its compiled scripts). Add it here once known.
    [HttpPut("{baid}")]
    public async Task<IActionResult> SetFavoriteSongs(uint baid, List<uint> songs)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        if (songs.Distinct().Count() != songs.Count)
            return BadRequest(new { message = "A song is in the list twice." });
        var known = catalog.GetMusicDetailDictionary();
        foreach (var id in songs)
        {
            if (!known.ContainsKey(id))
                return BadRequest(new { message = $"Song {id} isn't on this server." });
        }

        var user = await context.UserData.FindAsync([baid], HttpContext.RequestAborted);
        if (user is null)
        {
            return NotFound();
        }

        user.FavoriteSongsArray = songs;
        this.LogIfSomeoneElse(context, "favourites.edit", baid, $"{songs.Count} songs");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }
}
