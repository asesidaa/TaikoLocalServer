namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public partial class FavoriteSongsController
{
    private async Task<IActionResult> UpdateMomoiroFavoriteSong(SetFavoriteRequest request)
    {
        _ = catalog.For(GameEra.Momoiro);
        var existing = await context.MomoiroFavoriteSongs.FindAsync([request.Baid, request.SongId], HttpContext.RequestAborted);
        if (request.IsFavorite)
        {
            if (existing is not null)
                return NoContent();

            var count = await context.MomoiroFavoriteSongs.CountAsync(row => row.Baid == request.Baid, HttpContext.RequestAborted);
            var maxFavorites = GetAc15MaxFavoriteSongs(GameEra.Momoiro);
            if (count >= maxFavorites)
                return BadRequest($"Momoiro supports at most {maxFavorites} favorite songs.");

            var maxDisplayOrder = await context.MomoiroFavoriteSongs
                .Where(row => row.Baid == request.Baid)
                .Select(row => (int?)row.DisplayOrder)
                .MaxAsync(HttpContext.RequestAborted);
            context.MomoiroFavoriteSongs.Add(new MomoiroFavoriteSongs
            {
                Baid = request.Baid,
                SongNo = request.SongId,
                DisplayOrder = (maxDisplayOrder ?? -1) + 1
            });
        }
        else if (existing is not null)
        {
            context.MomoiroFavoriteSongs.Remove(existing);
        }

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private async Task<List<uint>> GetMomoiroFavoriteSongs(uint baid)
    {
        _ = catalog.For(GameEra.Momoiro);
        return await context.MomoiroFavoriteSongs
            .Where(row => row.Baid == baid)
            .OrderBy(row => row.DisplayOrder)
            .ThenBy(row => row.SongNo)
            .Select(row => row.SongNo)
            .ToListAsync(HttpContext.RequestAborted);
    }
}
