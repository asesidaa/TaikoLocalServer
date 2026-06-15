namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public partial class FavoriteSongsController
{
    private async Task<IActionResult> UpdateRedFavoriteSong(SetFavoriteRequest request)
    {
        _ = catalog.For(GameEra.Red);
        var existing = await context.RedFavoriteSongs.FindAsync([request.Baid, request.SongId], HttpContext.RequestAborted);
        if (request.IsFavorite)
        {
            if (existing is not null)
                return NoContent();

            var count = await context.RedFavoriteSongs.CountAsync(row => row.Baid == request.Baid, HttpContext.RequestAborted);
            var maxFavorites = GetAc15MaxFavoriteSongs(GameEra.Red);
            if (count >= maxFavorites)
                return BadRequest($"Red supports at most {maxFavorites} favorite songs.");

            context.RedFavoriteSongs.Add(new RedFavoriteSongs { Baid = request.Baid, SongNo = request.SongId });
        }
        else if (existing is not null)
        {
            context.RedFavoriteSongs.Remove(existing);
        }

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private async Task<List<uint>> GetRedFavoriteSongs(uint baid)
    {
        _ = catalog.For(GameEra.Red);
        return await context.RedFavoriteSongs
            .Where(row => row.Baid == baid)
            .Select(row => row.SongNo)
            .ToListAsync(HttpContext.RequestAborted);
    }
}
