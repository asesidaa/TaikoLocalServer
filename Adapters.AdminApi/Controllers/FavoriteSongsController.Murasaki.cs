namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public partial class FavoriteSongsController
{
    private async Task<IActionResult> UpdateMurasakiFavoriteSong(SetFavoriteRequest request)
    {
        _ = catalog.For(GameEra.Murasaki);
        var existing = await context.MurasakiFavoriteSongs.FindAsync([request.Baid, request.SongId], HttpContext.RequestAborted);
        if (request.IsFavorite)
        {
            if (existing is not null)
                return NoContent();

            var count = await context.MurasakiFavoriteSongs.CountAsync(row => row.Baid == request.Baid, HttpContext.RequestAborted);
            var maxFavorites = GetAc15MaxFavoriteSongs(GameEra.Murasaki);
            if (count >= maxFavorites)
                return BadRequest($"Murasaki supports at most {maxFavorites} favorite songs.");

            context.MurasakiFavoriteSongs.Add(new MurasakiFavoriteSongs { Baid = request.Baid, SongNo = request.SongId });
        }
        else if (existing is not null)
        {
            context.MurasakiFavoriteSongs.Remove(existing);
        }

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private async Task<List<uint>> GetMurasakiFavoriteSongs(uint baid)
    {
        _ = catalog.For(GameEra.Murasaki);
        return await context.MurasakiFavoriteSongs
            .Where(row => row.Baid == baid)
            .Select(row => row.SongNo)
            .ToListAsync(HttpContext.RequestAborted);
    }
}
