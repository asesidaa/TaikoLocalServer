namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public partial class FavoriteSongsController
{
    private async Task<IActionResult> UpdateWhiteFavoriteSong(SetFavoriteRequest request)
    {
        _ = catalog.For(GameEra.White);
        var existing = await context.WhiteFavoriteSongs.FindAsync([request.Baid, request.SongId], HttpContext.RequestAborted);
        if (request.IsFavorite)
        {
            if (existing is not null)
                return NoContent();

            var count = await context.WhiteFavoriteSongs.CountAsync(row => row.Baid == request.Baid, HttpContext.RequestAborted);
            var maxFavorites = GetAc15MaxFavoriteSongs(GameEra.White);
            if (count >= maxFavorites)
                return BadRequest($"White supports at most {maxFavorites} favorite songs.");

            context.WhiteFavoriteSongs.Add(new WhiteFavoriteSongs { Baid = request.Baid, SongNo = request.SongId });
        }
        else if (existing is not null)
        {
            context.WhiteFavoriteSongs.Remove(existing);
        }

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private async Task<List<uint>> GetWhiteFavoriteSongs(uint baid)
    {
        _ = catalog.For(GameEra.White);
        return await context.WhiteFavoriteSongs
            .Where(row => row.Baid == baid)
            .Select(row => row.SongNo)
            .ToListAsync(HttpContext.RequestAborted);
    }
}
