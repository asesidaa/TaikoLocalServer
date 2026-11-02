namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public partial class FavoriteSongsController
{
    private async Task<IActionResult> UpdateKimidoriFavoriteSong(SetFavoriteRequest request)
    {
        _ = catalog.For(GameEra.Kimidori);
        var existing = await context.KimidoriFavoriteSongs.FindAsync([request.Baid, request.SongId], HttpContext.RequestAborted);
        if (request.IsFavorite)
        {
            if (existing is not null)
                return NoContent();

            var count = await context.KimidoriFavoriteSongs.CountAsync(row => row.Baid == request.Baid, HttpContext.RequestAborted);
            var maxFavorites = GetAc15MaxFavoriteSongs(GameEra.Kimidori);
            if (count >= maxFavorites)
                return BadRequest($"Kimidori supports at most {maxFavorites} favorite songs.");

            context.KimidoriFavoriteSongs.Add(new KimidoriFavoriteSongs { Baid = request.Baid, SongNo = request.SongId });
        }
        else if (existing is not null)
        {
            context.KimidoriFavoriteSongs.Remove(existing);
        }

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private async Task<List<uint>> GetKimidoriFavoriteSongs(uint baid)
    {
        _ = catalog.For(GameEra.Kimidori);
        return await context.KimidoriFavoriteSongs
            .Where(row => row.Baid == baid)
            .Select(row => row.SongNo)
            .ToListAsync(HttpContext.RequestAborted);
    }
}
