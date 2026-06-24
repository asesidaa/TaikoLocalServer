namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public partial class PlayHistoryController
{
    private async Task<SongHistoryResponse> BuildKimidoriSongHistory(uint baid)
    {
        var playLogs = await context.SongPlayDataKimidori
            .Where(d => d.Baid == baid)
            .AsNoTracking()
            .ToListAsync(HttpContext.RequestAborted);
        var favoriteSet = await context.KimidoriFavoriteSongs
            .Where(d => d.Baid == baid)
            .Select(d => d.SongNo)
            .ToHashSetAsync(HttpContext.RequestAborted);

        var songHistory = playLogs.Select(play => new SongHistoryData
            {
                SongId = play.SongId,
                Difficulty = play.Difficulty,
                Score = play.Score,
                ScoreRank = ScoreRank.None,
                Crown = play.Crown,
                GoodCount = play.GoodCount,
                OkCount = play.OkCount,
                MissCount = play.MissCount,
                HitCount = play.HitCount,
                DrumrollCount = play.PoundCount,
                ComboCount = play.ComboCount,
                PlayTime = play.PlayTime,
                SongNumber = play.SongId,
                IsFavorite = favoriteSet.Contains(play.SongId)
            })
            .ToList();

        return new SongHistoryResponse { SongHistoryData = songHistory };
    }
}
