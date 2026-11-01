namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>A player across the whole server, for Play Data › Stats (the rest of that page is worked out from their own plays).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiScope(ApiScopes.ReadScores, ApiScopes.AdminUsers)]
public class StatsController(ITaikoDbContext context) : BaseAdminController<StatsController>
{
    private const int ClosestPlaces = 3;

    [HttpGet("{baid}/Server")]
    public async Task<ActionResult<PlayerServerStats>> Server(uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        var ct = HttpContext.RequestAborted;

        // Per player: charts (their bests, as the leaderboards and the Crowns table count them) and plays.
        var charts = await context.SongBestData.GroupBy(b => b.Baid).Select(g => new
        {
            Baid = g.Key, Played = g.Count(), Cleared = g.Count(b => b.BestCrown >= CrownType.Clear),
            FullCombos = g.Count(b => b.BestCrown >= CrownType.Gold),
        }).ToListAsync(ct);
        // shortcut: a credit is counted at its first song (SongNumber 0), so one whose first song wasn't saved is missed;
        // PlayHistory numbers credits exactly by walking the plays in order, too slow for every player at once.
        var plays = await context.SongPlayData.Where(p => !p.Skipped).GroupBy(p => p.Baid).Select(g => new
        {
            Baid = g.Key, Good = g.Sum(p => (long)p.GoodCount), Ok = g.Sum(p => (long)p.OkCount), Bad = g.Sum(p => (long)p.MissCount),
            Credits = g.Count(p => p.SongNumber == 0),
        }).ToDictionaryAsync(p => p.Baid, ct);

        PlayerTotals Totals(uint who)
        {
            var c = charts.FirstOrDefault(x => x.Baid == who);
            var p = plays.GetValueOrDefault(who);
            var notes = p is null ? 0 : p.Good + p.Ok + p.Bad;
            return new PlayerTotals
            {
                Cleared = c?.Cleared ?? 0, ClearRate = c is { Played: > 0 } ? c.Cleared * 100.0 / c.Played : 0,
                FullCombos = c?.FullCombos ?? 0, Accuracy = notes > 0 ? (p!.Good + p.Ok / 2.0) / notes * 100 : 0, Credits = p?.Credits ?? 0,
            };
        }

        var others = charts.Where(c => c.Baid != baid).Select(c => Totals(c.Baid)).ToList();
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
        var me = charts.FirstOrDefault(c => c.Baid == baid);
        if (me is null) return stats;

        // The clears ranking: places shared on a tie, as on the leaderboards.
        int RankOf(int cleared) => 1 + charts.Count(c => c.Cleared > cleared);
        stats.Rank = RankOf(me.Cleared);
        var order = charts.OrderByDescending(c => c.Cleared).ThenBy(c => c.Baid).ToList();
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
}
