using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>
/// A player's favourite songs, in order: Nijiiro's cabinet Favourites folder lists them as the array is stored
/// (UserSaveDataNijiiro.FavoriteSongsArray, sent as AryFavoriteSongNoes). The hearts add or remove one; the Favourites
/// page saves Nijiiro's whole ordered list. Older games keep theirs per era.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiScope(ApiScopes.ReadScores, ApiScopes.AdminUsers)]
public partial class FavoriteSongsController(ITaikoDbContext context, IGameDataCatalog catalog) : BaseAdminController<FavoriteSongsController>
{
    private readonly ITaikoDbContext context = context;
    private readonly IGameDataCatalog catalog = catalog;

    [HttpPost]
    public Task<IActionResult> UpdateFavoriteSong(SetFavoriteRequest request)
        => UpdateFavoriteSong(nameof(GameEra.Nijiiro), request);

    [HttpPost("/api/{era}/[controller]")]
    public async Task<IActionResult> UpdateFavoriteSong(string era, SetFavoriteRequest request)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
            return EraRoute.BadEra(era);

        if (this.AuthorizeOwnerOrAdmin(request.Baid) is { } forbid)
            return forbid;

        var user = await context.UserData.FindAsync(request.Baid);
        if (user is null)
        {
            return NotFound();
        }

        return gameEra switch
        {
            GameEra.Nijiiro => await UpdateNijiiroFavoriteSong(request),
            GameEra.Green => await UpdateGreenFavoriteSong(request),
            GameEra.Blue => await UpdateBlueFavoriteSong(request),
            GameEra.Yellow => await UpdateYellowFavoriteSong(request),
            GameEra.Red => await UpdateRedFavoriteSong(request),
            GameEra.White => await UpdateWhiteFavoriteSong(request),
            GameEra.Murasaki => await UpdateMurasakiFavoriteSong(request),
            GameEra.Kimidori => await UpdateKimidoriFavoriteSong(request),
            GameEra.Momoiro => await UpdateMomoiroFavoriteSong(request),
            _ => EraRoute.BadEra(era)
        };
    }

    private async Task<IActionResult> UpdateNijiiroFavoriteSong(SetFavoriteRequest request)
    {
        var saveData = await context.GetOrCreateNijiiroSaveDataAsync(request.Baid, HttpContext.RequestAborted);
        // Keeps the order: a new one goes last, a removed one leaves the rest where they were.
        var favorites = saveData.FavoriteSongsArray.ToList();
        if (request.IsFavorite && !favorites.Contains(request.SongId))
        {
            favorites.Add(request.SongId);
        }
        else if (!request.IsFavorite)
        {
            favorites.Remove(request.SongId);
        }

        saveData.FavoriteSongsArray = favorites;
        this.LogIfSomeoneElse(context, "favourites.edit", request.Baid, $"#{request.SongId} {(request.IsFavorite ? "added" : "removed")}");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private async Task<IActionResult> UpdateGreenFavoriteSong(SetFavoriteRequest request)
    {
        _ = catalog.For(GameEra.Green);
        var existing = await context.GreenFavoriteSongs.FindAsync([request.Baid, request.SongId], HttpContext.RequestAborted);
        if (request.IsFavorite)
        {
            if (existing is not null)
                return NoContent();

            var count = await context.GreenFavoriteSongs.CountAsync(row => row.Baid == request.Baid, HttpContext.RequestAborted);
            var maxFavorites = GetAc15MaxFavoriteSongs(GameEra.Green);
            if (count >= maxFavorites)
                return BadRequest($"Green supports at most {maxFavorites} favorite songs.");

            context.GreenFavoriteSongs.Add(new GreenFavoriteSongs { Baid = request.Baid, SongNo = request.SongId });
        }
        else if (existing is not null)
        {
            context.GreenFavoriteSongs.Remove(existing);
        }

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private async Task<IActionResult> UpdateBlueFavoriteSong(SetFavoriteRequest request)
    {
        _ = catalog.For(GameEra.Blue);
        var existing = await context.BlueFavoriteSongs.FindAsync([request.Baid, request.SongId], HttpContext.RequestAborted);
        if (request.IsFavorite)
        {
            if (existing is not null)
                return NoContent();

            var count = await context.BlueFavoriteSongs.CountAsync(row => row.Baid == request.Baid, HttpContext.RequestAborted);
            var maxFavorites = GetAc15MaxFavoriteSongs(GameEra.Blue);
            if (count >= maxFavorites)
                return BadRequest($"Blue supports at most {maxFavorites} favorite songs.");

            context.BlueFavoriteSongs.Add(new BlueFavoriteSongs { Baid = request.Baid, SongNo = request.SongId });
        }
        else if (existing is not null)
        {
            context.BlueFavoriteSongs.Remove(existing);
        }

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpGet("{baid}")]
    public Task<IActionResult> GetFavoriteSongs(uint baid)
        => GetFavoriteSongs(nameof(GameEra.Nijiiro), baid);

    [HttpGet("/api/{era}/[controller]/{baid}")]
    public async Task<IActionResult> GetFavoriteSongs(string era, uint baid)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
            return EraRoute.BadEra(era);

        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }

        return gameEra switch
        {
            GameEra.Nijiiro => Ok((await context.GetOrCreateNijiiroSaveDataAsync(baid, HttpContext.RequestAborted)).FavoriteSongsArray),
            GameEra.Green => Ok(await context.GreenFavoriteSongs
                .Where(row => row.Baid == baid)
                .Select(row => row.SongNo)
                .ToListAsync(HttpContext.RequestAborted)),
            GameEra.Blue => Ok(await context.BlueFavoriteSongs
                .Where(row => row.Baid == baid)
                .Select(row => row.SongNo)
                .ToListAsync(HttpContext.RequestAborted)),
            GameEra.Yellow => Ok(await GetYellowFavoriteSongs(baid)),
            GameEra.Red => Ok(await GetRedFavoriteSongs(baid)),
            GameEra.White => Ok(await GetWhiteFavoriteSongs(baid)),
            GameEra.Murasaki => Ok(await GetMurasakiFavoriteSongs(baid)),
            GameEra.Kimidori => Ok(await GetKimidoriFavoriteSongs(baid)),
            GameEra.Momoiro => Ok(await GetMomoiroFavoriteSongs(baid)),
            _ => EraRoute.BadEra(era)
        };
    }

    /// <summary>Nijiiro's whole list, in the order the Favourites folder should show it (the Favourites page's Save).</summary>
    // shortcut: no cap on the count; the game may have one (not found in its compiled scripts). Add it here once known.
    [HttpPut("{baid}")]
    public async Task<IActionResult> SetFavoriteSongs(uint baid, List<uint> songs)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        if (songs.Distinct().Count() != songs.Count)
            return BadRequest(new { message = "A song is in the list twice." });
        var known = catalog.Nijiiro().GetMusicDetailDictionary();
        // Songs already in the list may stay though the Web UI no longer lists them (left out of music_order since).
        var had = (await context.UserSaveDataNijiiro.AsNoTracking().FirstOrDefaultAsync(s => s.Baid == baid, HttpContext.RequestAborted))
            ?.FavoriteSongsArray.ToHashSet() ?? [];
        foreach (var id in songs)
        {
            if (!known.ContainsKey(id) && !had.Contains(id))
                return BadRequest(new { message = $"Song {id} isn't on this server." });
        }

        if (await context.UserData.FindAsync([baid], HttpContext.RequestAborted) is null)
        {
            return NotFound();
        }

        var saveData = await context.GetOrCreateNijiiroSaveDataAsync(baid, HttpContext.RequestAborted);
        saveData.FavoriteSongsArray = songs;
        this.LogIfSomeoneElse(context, "favourites.edit", baid, $"{songs.Count} songs");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private static int GetAc15MaxFavoriteSongs(GameEra era)
        => Ac15EraProfiles.GetMaxFavoriteSongs(era)
           ?? throw new ArgumentOutOfRangeException(nameof(era), era, "Era does not use AC15 favorite limits.");
}
