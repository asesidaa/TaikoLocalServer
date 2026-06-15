namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public partial class FavoriteSongsController
{
    private async Task<IActionResult> UpdateYellowFavoriteSong(SetFavoriteRequest request)
    {
        _ = catalog.For(GameEra.Yellow);
        var existing = await context.YellowFavoriteSongs.FindAsync([request.Baid, request.SongId], HttpContext.RequestAborted);
        if (request.IsFavorite)
        {
            if (existing is not null)
                return NoContent();

            var count = await context.YellowFavoriteSongs.CountAsync(row => row.Baid == request.Baid, HttpContext.RequestAborted);
            var maxFavorites = GetAc15MaxFavoriteSongs(GameEra.Yellow);
            if (count >= maxFavorites)
                return BadRequest($"Yellow supports at most {maxFavorites} favorite songs.");

            context.YellowFavoriteSongs.Add(new YellowFavoriteSongs { Baid = request.Baid, SongNo = request.SongId });
        }
        else if (existing is not null)
        {
            context.YellowFavoriteSongs.Remove(existing);
        }

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private async Task<List<uint>> GetYellowFavoriteSongs(uint baid)
    {
        _ = catalog.For(GameEra.Yellow);
        return await context.YellowFavoriteSongs
            .Where(row => row.Baid == baid)
            .Select(row => row.SongNo)
            .ToListAsync(HttpContext.RequestAborted);
    }
}
