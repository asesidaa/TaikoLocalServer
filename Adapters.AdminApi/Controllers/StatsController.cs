using Microsoft.Extensions.Caching.Memory;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>A player across the whole server, for Play Data › Stats (the rest of that page is worked out from their own plays).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiScope(ApiScopes.ReadScores, ApiScopes.AdminUsers)]
public class StatsController(ITaikoDbContext context, IMemoryCache cache) : BaseAdminController<StatsController>
{
    private const int ClosestPlaces = 3;

    private sealed record ChartTotals(uint Baid, int Played, int Cleared, int FullCombos);
    private sealed record PlayTotals(uint Baid, long Good, long Ok, long Bad);

    [HttpGet("{baid}/Server")]
    public async Task<ActionResult<PlayerServerStats>> Server(uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        var ct = HttpContext.RequestAborted;

        // Every player's totals: the same for every viewer and a scan of every best and play, so they're kept 5 minutes.
        // The viewer's own come fresh below, so they always match the rest of their Stats page.
        var (allCharts, allPlays, allCredits) = (await cache.GetOrCreateAsync("stats.server", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return (await ChartTotalsQuery(context.SongBestData).ToListAsync(ct), await PlayTotalsQuery(context.SongPlayData).ToListAsync(ct),
                await context.CreditCountsAsync(null, PlayHistoryController.SessionGap, ct));
        }))!;
        var charts = allCharts.ToDictionary(c => c.Baid);
        var plays = allPlays.ToDictionary(p => p.Baid);
        var credits = new Dictionary<uint, int>(allCredits);
        charts.Remove(baid);
        plays.Remove(baid);
        credits.Remove(baid);
        if (await ChartTotalsQuery(context.SongBestData.Where(b => b.Baid == baid)).FirstOrDefaultAsync(ct) is { } myCharts) charts[baid] = myCharts;
        if (await PlayTotalsQuery(context.SongPlayData.Where(p => p.Baid == baid)).FirstOrDefaultAsync(ct) is { } myPlays) plays[baid] = myPlays;
        foreach (var (who, n) in await context.CreditCountsAsync(baid, PlayHistoryController.SessionGap, ct)) credits[who] = n;

        PlayerTotals Totals(uint who)
        {
            var c = charts.GetValueOrDefault(who);
            var p = plays.GetValueOrDefault(who);
            var notes = p is null ? 0 : p.Good + p.Ok + p.Bad;
            return new PlayerTotals
            {
                Cleared = c?.Cleared ?? 0, ClearRate = c is { Played: > 0 } ? c.Cleared * 100.0 / c.Played : 0,
                FullCombos = c?.FullCombos ?? 0, Accuracy = notes > 0 ? (p!.Good + p.Ok / 2.0) / notes * 100 : 0, Credits = credits.GetValueOrDefault(who),
            };
        }

        var others = charts.Keys.Where(b => b != baid).Select(Totals).ToList();
        var stats = new PlayerServerStats
        {
            Players = charts.Count,
            Mine = Totals(baid),
            Average = others.Count == 0 ? new PlayerTotals() : new PlayerTotals
            {
                Cleared = others.Average(o => o.Cleared), ClearRate = others.Average(o => o.ClearRate),
                FullCombos = others.Average(o => o.FullCombos), Accuracy = others.Average(o => o.Accuracy), Credits = others.Average(o => o.Credits),
            },
        };
        if (charts.GetValueOrDefault(baid) is not { } me) return stats;

        // The clears ranking: places shared on a tie, as on the leaderboards.
        int RankOf(int cleared) => 1 + charts.Values.Count(c => c.Cleared > cleared);
        stats.Rank = RankOf(me.Cleared);
        var order = charts.Values.OrderByDescending(c => c.Cleared).ThenBy(c => c.Baid).ToList();
        var at = order.FindIndex(c => c.Baid == baid);
        var near = order.Skip(Math.Max(0, at - 2)).Take(Math.Min(at, 2) + 3).ToList();
        var nearBaids = near.Select(c => c.Baid).ToList();
        var names = await context.UserData.Where(u => nearBaids.Contains(u.Baid)).ToDictionaryAsync(u => u.Baid, u => u.MyDonName, ct);
        stats.Neighbours = near.Select(c => new StandingRow
        {
            Rank = RankOf(c.Cleared), Baid = c.Baid, Cleared = c.Cleared,
            Name = names.GetValueOrDefault(c.Baid) is { Length: > 0 } n ? n : $"#{c.Baid}",
        }).ToList();

        // Per chart of theirs, the players ahead: how many and the lowest of their bests (the place to take next).
        var ahead = await (
            from m in context.SongBestData.Where(m => m.Baid == baid)
            from o in context.SongBestData
            where o.SongId == m.SongId && o.Difficulty == m.Difficulty && o.BestScore > m.BestScore
            group o.BestScore by new { m.SongId, m.Difficulty, m.BestScore } into g
            select new { g.Key.SongId, g.Key.Difficulty, Mine = g.Key.BestScore, Count = g.Count(), Next = g.Min() }
        ).ToListAsync(ct);
        stats.First = me.Played - ahead.Count;
        stats.Top3 = me.Played - ahead.Count(a => a.Count >= 3);
        stats.Top10 = me.Played - ahead.Count(a => a.Count >= 10);

        foreach (var a in ahead.OrderBy(a => a.Next - a.Mine).Take(ClosestPlaces))
        {
            var them = await context.SongBestData.Where(o => o.SongId == a.SongId && o.Difficulty == a.Difficulty && o.BestScore == a.Next)
                .OrderBy(o => o.Baid).Select(o => o.Baid).FirstAsync(ct);
            var name = await context.UserData.Where(u => u.Baid == them).Select(u => u.MyDonName).FirstOrDefaultAsync(ct);
            var theirRank = 1 + await context.SongBestData.CountAsync(o => o.SongId == a.SongId && o.Difficulty == a.Difficulty && o.BestScore > a.Next, ct);
            stats.ClosestPlaces.Add(new PlaceGoal
            {
                SongId = a.SongId, Difficulty = a.Difficulty, Gap = a.Next - a.Mine + 1,
                Name = string.IsNullOrEmpty(name) ? $"#{them}" : name, Rank = theirRank,
            });
        }
        return stats;
    }

    /// <summary>This player's own Stats, summed here (see <see cref="PlayerStatsSummary"/>).</summary>
    [HttpGet("{baid}")]
    public async Task<ActionResult<PlayerStatsSummary>> Summary(uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        var ct = HttpContext.RequestAborted;
        // Every play counts, skipped ones too, as api/PlayHistory (which this page used to sum) returns them.
        var plays = context.SongPlayData.Where(p => p.Baid == baid);
        var today = DateTime.Now.Date;

        var bests = await context.SongBestData.Where(b => b.Baid == baid)
            .Select(b => new StatsBest(b.SongId, b.Difficulty, b.BestCrown, b.BestScoreRank)).ToListAsync(ct);
        var totals = await plays.GroupBy(p => p.Baid).Select(g => new
        {
            Plays = g.Count(), Week = g.Count(p => p.PlayTime >= today.AddDays(-6)), Combo = g.Max(p => p.ComboCount),
            Good = g.Sum(p => (long)p.GoodCount), Ok = g.Sum(p => (long)p.OkCount), Bad = g.Sum(p => (long)p.MissCount),
        }).FirstOrDefaultAsync(ct);
        var summary = new PlayerStatsSummary { Bests = bests };
        if (totals is null) return summary;
        summary.Plays = totals.Plays;
        summary.PlaysThisWeek = totals.Week;
        summary.BestCombo = totals.Combo;
        (summary.Good, summary.Ok, summary.Bad) = (totals.Good, totals.Ok, totals.Bad);

        summary.Days = await plays.Where(p => p.PlayTime >= today.AddDays(-7 * 53)).GroupBy(p => p.PlayTime.Date)
            .Select(g => new DayPlays(g.Key, g.Count())).ToListAsync(ct);
        summary.Songs = await plays.GroupBy(p => p.SongId).Select(g => new SongPlays(g.Key, g.Count())).ToListAsync(ct);

        // Closest full combos: per chart without one, the play with the fewest Bads, then the fewest OKs. Both in one
        // minimum (Bads × 1,000,000 + OKs: a chart never has a million notes).
        const long okSpan = 1_000_000;
        var fullCombos = bests.Where(b => b.BestCrown >= CrownType.Gold).Select(b => (b.SongId, b.Difficulty)).ToHashSet();
        summary.ClosestFullCombos = (await plays.Where(p => p.MissCount > 0).GroupBy(p => new { p.SongId, p.Difficulty })
                .Select(g => new { g.Key.SongId, g.Key.Difficulty, Key = g.Min(p => (long)p.MissCount * okSpan + p.OkCount) }).ToListAsync(ct))
            .Where(c => !fullCombos.Contains((c.SongId, c.Difficulty))).OrderBy(c => c.Key).Take(2)
            .Select(c => new FullComboGoal(c.SongId, c.Difficulty, (uint)(c.Key / okSpan), (uint)(c.Key % okSpan))).ToList();

        // Recent personal bests: the last 14 days' plays that beat everything before them on their chart.
        var since = today.AddDays(-13);
        var recent = await plays.Where(p => p.PlayTime >= since).OrderBy(p => p.PlayTime)
            .Select(p => new { p.SongId, p.Difficulty, p.PlayTime, p.Score, p.Crown }).ToListAsync(ct);
        var songs = recent.Select(p => p.SongId).Distinct().ToList();
        var before = (await plays.Where(p => p.PlayTime < since && songs.Contains(p.SongId))
                .GroupBy(p => new { p.SongId, p.Difficulty }).Select(g => new { g.Key.SongId, g.Key.Difficulty, Top = g.Max(p => p.Score) })
                .ToListAsync(ct))
            .ToDictionary(c => (c.SongId, c.Difficulty), c => (uint?)c.Top);
        var latest = new Dictionary<(uint, Difficulty), RecentBest>();
        foreach (var p in recent)
        {
            var key = (p.SongId, p.Difficulty);
            var top = before.GetValueOrDefault(key);
            if (top is { } prior && p.Score > prior) latest[key] = new RecentBest(p.SongId, p.Difficulty, p.PlayTime, p.Score, p.Score - prior, p.Crown);
            before[key] = Math.Max(top ?? 0, p.Score);
        }
        summary.RecentBests = latest.Values.OrderByDescending(b => b.PlayTime).Take(12).ToList();
        return summary;
    }

    // Per player: their charts (bests, as the leaderboards and the Crowns table count them) and their plays. One query
    // shape for everyone (cached) and for the viewer (fresh), so the two agree.
    private static IQueryable<ChartTotals> ChartTotalsQuery(IQueryable<SongBestDatum> bests) =>
        bests.GroupBy(b => b.Baid).Select(g => new ChartTotals(g.Key, g.Count(),
            g.Count(b => b.BestCrown >= CrownType.Clear), g.Count(b => b.BestCrown >= CrownType.Gold)));

    // Credits aren't here: they need the plays in order (ITaikoDbContext.CreditCountsAsync), as Play History counts them.
    private static IQueryable<PlayTotals> PlayTotalsQuery(IQueryable<SongPlayDatum> plays) =>
        plays.Where(p => !p.Skipped).GroupBy(p => p.Baid).Select(g => new PlayTotals(g.Key, g.Sum(p => (long)p.GoodCount),
            g.Sum(p => (long)p.OkCount), g.Sum(p => (long)p.MissCount)));
}
