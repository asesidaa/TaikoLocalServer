namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public partial class PlayDataController
{
    private async Task<SongBestResponse> BuildWhiteSongBestResponse(uint baid)
    {
        var bestRows = await context.SongBestDataWhite
            .Where(d => d.Baid == baid)
            .AsNoTracking()
            .ToListAsync(HttpContext.RequestAborted);
        var playRows = await context.SongPlayDataWhite
            .Where(d => d.Baid == baid)
            .AsNoTracking()
            .ToListAsync(HttpContext.RequestAborted);
        var favoriteSet = await context.WhiteFavoriteSongs
            .Where(d => d.Baid == baid)
            .Select(d => d.SongNo)
            .ToHashSetAsync(HttpContext.RequestAborted);

        var songBestRecords = bestRows
            .GroupBy(d => new { d.SongId, d.Difficulty })
            .Select(group => BuildWhiteSongBestData(group, playRows, favoriteSet))
            .ToList();

        return new SongBestResponse
        {
            SongBestData = songBestRecords
        };
    }

    private static SongBestData BuildWhiteSongBestData(
        IEnumerable<SongBestDatumWhite> bestRows,
        IReadOnlyCollection<SongPlayDatumWhite> playRows,
        IReadOnlySet<uint> favoriteSet)
    {
        var rows = bestRows.ToList();
        var primary = rows.FirstOrDefault(d => !d.IsShin) ?? rows.First();
        var alternate = rows.FirstOrDefault(d => d.IsShin && d != primary);
        var matchingPlays = playRows
            .Where(d => d.SongId == primary.SongId && d.Difficulty == primary.Difficulty)
            .ToList();
        var bestPlay = matchingPlays.MaxBy(d => d.Score);

        var result = new SongBestData
        {
            SongId = primary.SongId,
            Difficulty = primary.Difficulty,
            BestScore = primary.BestScore,
            BestRate = primary.BestRate,
            BestCrown = primary.BestCrown,
            BestScoreRank = ScoreRank.None,
            IsFavorite = favoriteSet.Contains(primary.SongId),
            PlayCount = matchingPlays.Count,
            ClearCount = matchingPlays.Count(d => d.Crown >= CrownType.Clear),
            FullComboCount = matchingPlays.Count(d => d.Crown >= CrownType.Gold),
            PerfectCount = matchingPlays.Count(d => d.Crown >= CrownType.Dondaful),
            RecentPlayData = matchingPlays.Select(MapWhitePlayToDto).ToList()
        };

        if (matchingPlays.Count > 0)
        {
            result.LastPlayTime = matchingPlays.Max(d => d.PlayTime);
        }

        if (bestPlay is not null)
        {
            result.PlayTime = bestPlay.PlayTime;
            result.GoodCount = bestPlay.GoodCount;
            result.OkCount = bestPlay.OkCount;
            result.MissCount = bestPlay.MissCount;
            result.ComboCount = bestPlay.ComboCount;
            result.HitCount = bestPlay.HitCount;
            result.DrumrollCount = bestPlay.PoundCount;
        }

        if (alternate is not null)
        {
            result.AlternateScore = new ScoreFacet
            {
                Label = "Shin",
                BestScore = alternate.BestScore,
                BestRate = alternate.BestRate,
                BestCrown = alternate.BestCrown
            };
        }

        return result;
    }

    private static SongPlayDatumDto MapWhitePlayToDto(SongPlayDatumWhite play)
    {
        return new SongPlayDatumDto
        {
            SongId = play.SongId,
            SongNumber = play.SongId,
            Difficulty = play.Difficulty,
            Crown = play.Crown,
            Score = play.Score,
            ScoreRate = play.ScoreRate,
            ScoreRank = ScoreRank.None,
            GoodCount = play.GoodCount,
            OkCount = play.OkCount,
            MissCount = play.MissCount,
            ComboCount = play.ComboCount,
            HitCount = play.HitCount,
            DrumrollCount = play.PoundCount,
            PlayTime = play.PlayTime
        };
    }
}
