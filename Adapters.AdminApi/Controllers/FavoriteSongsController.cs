using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
            _ => EraRoute.BadEra(era)
        };
    }

    private async Task<IActionResult> UpdateNijiiroFavoriteSong(SetFavoriteRequest request)
    {
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
            _ => EraRoute.BadEra(era)
        };
    }

    private static int GetAc15MaxFavoriteSongs(GameEra era)
        => Ac15EraProfiles.GetMaxFavoriteSongs(era)
           ?? throw new ArgumentOutOfRangeException(nameof(era), era, "Era does not use AC15 favorite limits.");
}
