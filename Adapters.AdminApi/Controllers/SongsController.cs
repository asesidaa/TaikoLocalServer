using System.Linq.Expressions;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>
/// Song List and the Song page: every song in the catalog with one player's results (and every player's play count). The list is
/// filtered, sorted and paged here, so the Web UI never loads every song's results at once.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiScope(ApiScopes.ReadScores, ApiScopes.AdminUsers)]
public class SongsController(ITaikoDbContext context, IGameDataCatalog catalog) : BaseAdminController<SongsController>
{
    private const int MaxLimit = 200;

    /// <param name="diff">The difficulty (1 Easy … 5 Ura Oni) that <paramref name="status"/> and the stars sort refer to.</param>
    /// <param name="status">all, unplayed, notcleared, nofc; AI Battle: aiunplayed, ailost, aiwon.</param>
    /// <param name="isNew">Only songs the game marks new.</param>
    /// <param name="sort">id, title (A to Z), stars (high first), plays (most played by everyone first) or new (new songs first).</param>
    /// <param name="reverse">The other way round, e.g. Z to A.</param>
    [HttpGet("{baid}")]
    public async Task<ActionResult<SongListPage>> List(uint baid, [FromQuery] string? q = null, [FromQuery] int genre = -1,
        [FromQuery] bool fav = false, [FromQuery] bool isNew = false, [FromQuery] int diff = 4, [FromQuery] string? results = null,
        [FromQuery] string sort = "id", [FromQuery] bool reverse = false, [FromQuery] int page = 1, [FromQuery] int limit = 50)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        var user = await context.UserData.FindAsync([baid], HttpContext.RequestAborted);
        if (user is null) return NotFound();
        // Favourites and the Sou-uchi switch are Nijiiro's; a player who hasn't played it has the defaults.
        var save = await context.UserSaveDataNijiiro.AsNoTracking().FirstOrDefaultAsync(s => s.Baid == baid, HttpContext.RequestAborted) ?? new UserSaveDataNijiiro();
        if (diff is < 1 or > 5 || page < 1 || limit is < 1 or > MaxLimit) return BadRequest(new { message = "Bad paging or difficulty." });

        var songs = catalog.Nijiiro().GetMusicDetailDictionary().Values;
        var best = (await context.SongBestDataNijiiro.Where(b => b.Baid == baid).Select(b => new { b.SongId, b.Difficulty, b.BestCrown })
                .ToListAsync(HttpContext.RequestAborted))
            .ToDictionary(b => (b.SongId, b.Difficulty), b => b.BestCrown);
        var ai = (await context.AiScoreDataNijiiro.Where(a => a.Baid == baid).Include(a => a.AiSectionScoreData).ToListAsync(HttpContext.RequestAborted))
            .ToDictionary(a => (a.SongId, a.Difficulty), a => new AiResult
            {
                Win = a.IsWin, Sections = a.AiSectionScoreData.Count, SectionsWon = a.AiSectionScoreData.Count(s => s.IsWin),
            });
        var favorites = save.FavoriteSongsArray.ToHashSet();
        var d = (Difficulty)diff;

        var query = q?.Trim() ?? "";
        IEnumerable<MusicDetail> list = songs.Where(s =>
            (genre < 0 || (int)s.Genre == genre) &&
            (!fav || favorites.Contains(s.SongId)) &&
            (!isNew || s.IsNew) &&
            // Like the game: Sou-uchi charts only for players who turned them on (Profile › Show Sou-uchi songs).
            (save.DisplaySouUchi || !s.IsSouUchi) &&
            // The picked difficulty also filters: only songs with that chart (in practice Ura Oni).
            Stars(s)[diff - 1] > 0 &&
            (query.Length == 0 || s.SongId.ToString() == query ||
             s.SongName.Contains(query, StringComparison.OrdinalIgnoreCase) || s.SongNameEN.Contains(query, StringComparison.OrdinalIgnoreCase) ||
             s.SongNameCN.Contains(query, StringComparison.OrdinalIgnoreCase) || s.SongNameKO.Contains(query, StringComparison.OrdinalIgnoreCase)));
        // Your result on that chart: any of the ones picked (none picked: every song).
        var wanted = (results ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet();
        if (wanted.Count > 0)
            list = list.Where(s => wanted.Contains(ResultOf(s.SongId)));
        string ResultOf(uint songId)
        {
            if (ai.GetValueOrDefault((songId, d)) is var battle && wanted.Any(w => w.StartsWith("ai")))
                return battle is null ? "aiunplayed" : battle.Win ? "aiwon" : "ailost";
            return best.TryGetValue((songId, d), out var crown) ? crown.ToString().ToLowerInvariant() : "unplayed";
        }
        // Most played needs every song's count, not just this page's (read from the SongId/Skipped index).
        var allPlays = sort == "plays" ? await PlayCounts(null) : null;
        var language = await SongLanguage.ForAsync(context, User, baid, HttpContext.RequestAborted);
        list = sort switch
        {
            "title" => list.OrderBy(s => s.Title(language), SongLanguage.TitleOrder(language)).ThenBy(s => s.SongId),
            "stars" => list.OrderByDescending(s => Stars(s)[diff - 1]).ThenBy(s => s.SongId),
            "plays" => list.OrderByDescending(s => allPlays!.GetValueOrDefault(s.SongId)).ThenBy(s => s.SongId),
            "new" => list.OrderByDescending(s => s.IsNew).ThenBy(s => s.SongId),
            _ => list.OrderBy(s => s.SongId),
        };

        var all = list.ToList();
        if (reverse) all.Reverse();
        var pages = Math.Max(1, (all.Count + limit - 1) / limit);
        page = Math.Min(page, pages);
        var shown = all.Skip((page - 1) * limit).Take(limit).ToList();
        var totals = allPlays ?? await PlayCounts(shown.Select(s => s.SongId).ToList());
        return new SongListPage
        {
            Total = all.Count, AllSongs = songs.Count, Page = page, TotalPages = pages,
            Items = shown.Select(s => new SongListItem
            {
                SongId = s.SongId, Name = s.Title(language), NameEn = s.Subtitle(language), Genre = s.Genre, Favorite = favorites.Contains(s.SongId),
                IsNew = s.IsNew, TotalPlays = totals.GetValueOrDefault(s.SongId),
                Stars = Stars(s),
                Crowns = Enumerable.Range(1, 5).Select(i => best.TryGetValue((s.SongId, (Difficulty)i), out var c) ? c : (CrownType?)null).ToArray(),
                Ai = Enumerable.Range(1, 5).Select(i => ai.GetValueOrDefault((s.SongId, (Difficulty)i))).ToArray(),
            }).ToList(),
        };
    }

    [HttpGet("{baid}/{songId}")]
    public async Task<ActionResult<SongDetail>> Detail(uint baid, uint songId)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        var user = await context.UserData.FindAsync([baid], HttpContext.RequestAborted);
        if (user is null || !catalog.Nijiiro().GetMusicDetailDictionary().TryGetValue(songId, out var s)) return NotFound();
        var save = await context.UserSaveDataNijiiro.AsNoTracking().FirstOrDefaultAsync(x => x.Baid == baid, HttpContext.RequestAborted) ?? new UserSaveDataNijiiro();

        // One read of this player's plays of the song (just the numbers, in the order of the Baid/SongId/Difficulty/PlayTime
        // index) gives each chart's totals, best and score line; the plays themselves come a page at a time from Plays().
        var plays = (await context.SongPlayDataNijiiro.Where(p => p.Baid == baid && p.SongId == songId && !p.Skipped)
                .OrderBy(p => p.Difficulty).ThenBy(p => p.PlayTime)
                .Select(p => new { p.Difficulty, Play = new SongChartPlay
                {
                    PlayTime = p.PlayTime, Score = p.Score, Crown = p.Crown, Rank = p.ScoreRank,
                    Good = p.GoodCount, Ok = p.OkCount, Bad = p.MissCount, Combo = p.ComboCount, Drumroll = p.DrumrollCount,
                } }).ToListAsync(HttpContext.RequestAborted))
            .GroupBy(p => p.Difficulty).ToDictionary(g => g.Key, g => g.Select(p => p.Play).ToList());
        var ai = await context.AiScoreDataNijiiro.Where(a => a.Baid == baid && a.SongId == songId).Include(a => a.AiSectionScoreData)
            .ToListAsync(HttpContext.RequestAborted);
        var stars = Stars(s);
        var language = await SongLanguage.ForAsync(context, User, baid, HttpContext.RequestAborted);
        return new SongDetail
        {
            SongId = s.SongId, Name = s.Title(language), NameEn = s.Subtitle(language), Genre = s.Genre, Stars = stars,
            Favorite = save.FavoriteSongsArray.Contains(songId), IsNew = s.IsNew,
            TotalPlays = await context.SongPlayDataNijiiro.CountAsync(p => p.SongId == songId && !p.Skipped, HttpContext.RequestAborted),
            Charts = Enumerable.Range(1, 5).Where(i => stars[i - 1] > 0).Select(i =>
            {
                var d = (Difficulty)i;
                var battle = ai.FirstOrDefault(a => a.Difficulty == d);
                var chart = plays.GetValueOrDefault(d) ?? [];
                return new SongChart
                {
                    Difficulty = d, Stars = stars[i - 1],
                    // Oldest first, so MaxBy keeps the earliest of equal scores.
                    Best = chart.MaxBy(p => p.Score), Summary = Summarize(chart),
                    ScoreLine = ScoreLine(chart.Select(p => (p.PlayTime, p.Score)).ToList()),
                    AiWin = battle?.IsWin,
                    AiSections = battle?.AiSectionScoreData.OrderBy(x => x.SectionIndex).Select(x => new AiSection
                    {
                        Crown = x.Crown, Win = x.IsWin, Score = x.Score, Good = x.GoodCount, Ok = x.OkCount, Bad = x.MissCount, Drumroll = x.DrumrollCount,
                    }).ToList(),
                };
            }).ToList(),
        };
    }

    /// <summary>One chart across every player on the server, compared with this one (see <see cref="SongChartServerStats"/>).</summary>
    [HttpGet("{baid}/{songId}/Server")]
    public async Task<ActionResult<SongChartServerStats>> ServerStats(uint baid, uint songId, [FromQuery] int difficulty = 4)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        if (difficulty is < 1 or > 5) return BadRequest(new { message = "Bad difficulty." });

        var d = (Difficulty)difficulty;
        var ct = HttpContext.RequestAborted;
        // One row per player each: their best (the leaderboard's rows) and their play totals.
        var bests = await context.SongBestDataNijiiro.Where(b => b.SongId == songId && b.Difficulty == d)
            .Select(b => new { b.Baid, b.BestScore, b.BestCrown }).ToListAsync(ct);
        var players = await context.SongPlayDataNijiiro.Where(p => p.SongId == songId && p.Difficulty == d && !p.Skipped)
            .GroupBy(p => p.Baid).Select(g => new
            {
                Baid = g.Key, Plays = g.Count(), Good = g.Sum(p => (long)p.GoodCount), Ok = g.Sum(p => (long)p.OkCount),
                Bad = g.Sum(p => (long)p.MissCount), Last = g.Max(p => p.PlayTime),
            }).ToListAsync(ct);

        var bins = new int[1_000_000 / SongChartServerStats.BinWidth + 1];
        var crowns = new int[4];
        foreach (var b in bests)
        {
            bins[Math.Min(bins.Length - 1, (int)(b.BestScore / SongChartServerStats.BinWidth))]++;
            crowns[Math.Clamp((int)b.BestCrown, 0, 3)]++;
        }
        // The averages are everyone else's, so you're compared with the rest of the server.
        var otherBests = bests.Where(b => b.Baid != baid).ToList();
        var others = players.Where(p => p.Baid != baid).ToList();
        var accuracies = others.Where(p => p.Good + p.Ok + p.Bad > 0).Select(p => (p.Good + p.Ok / 2.0) / (p.Good + p.Ok + p.Bad) * 100).ToList();
        var stats = new SongChartServerStats
        {
            Players = bests.Count,
            AverageBest = otherBests.Count == 0 ? 0 : otherBests.Average(b => (double)b.BestScore),
            AverageAccuracy = accuracies.Count == 0 ? 0 : accuracies.Average(),
            AveragePlays = others.Count == 0 ? 0 : others.Average(p => p.Plays),
            Bins = bins, Crowns = crowns,
            TotalPlays = players.Sum(p => p.Plays),
            LastPlayed = players.Count == 0 ? null : players.Max(p => p.Last),
        };

        if (bests.FirstOrDefault(b => b.Baid == baid) is not { } mine) return stats;
        int RankOf(uint score) => 1 + bests.Count(b => b.BestScore > score);
        stats.Rank = RankOf(mine.BestScore);
        if (bests.Count > 1) stats.BetterThan = bests.Count(b => b.BestScore < mine.BestScore) * 100.0 / (bests.Count - 1);
        var neighbour = stats.Rank == 1
            ? bests.Where(b => b.Baid != baid).MaxBy(b => b.BestScore)
            : bests.Where(b => b.BestScore > mine.BestScore).MinBy(b => b.BestScore);
        if (neighbour is not null)
        {
            var name = await context.UserData.Where(u => u.Baid == neighbour.Baid).Select(u => u.MyDonName).FirstOrDefaultAsync(ct);
            var n = new ChartNeighbour { Name = string.IsNullOrEmpty(name) ? $"#{neighbour.Baid}" : name, Rank = RankOf(neighbour.BestScore), Score = neighbour.BestScore };
            if (stats.Rank == 1) stats.RunnerUp = n;
            else stats.Above = n;
        }
        return stats;
    }

    /// <summary>Every player's plays per song (skipped songs don't count), for <paramref name="songs"/> or every song.</summary>
    private Task<Dictionary<uint, int>> PlayCounts(List<uint>? songs) =>
        context.SongPlayDataNijiiro.Where(p => (songs == null || songs.Contains(p.SongId)) && !p.Skipped)
            .GroupBy(p => p.SongId).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, HttpContext.RequestAborted);

    /// <summary>One chart's plays, newest first, a page at a time (the Song page's play history).</summary>
    [HttpGet("{baid}/{songId}/Plays")]
    public async Task<ActionResult<SongPlaysPage>> Plays(uint baid, uint songId, [FromQuery] int difficulty = 4,
        [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        if (difficulty is < 1 or > 5 || page < 1 || limit is < 1 or > MaxLimit) return BadRequest(new { message = "Bad paging or difficulty." });

        var d = (Difficulty)difficulty;
        var plays = context.SongPlayDataNijiiro.Where(p => p.Baid == baid && p.SongId == songId && p.Difficulty == d && !p.Skipped);
        var total = await plays.CountAsync(HttpContext.RequestAborted);
        var pages = Math.Max(1, (total + limit - 1) / limit);
        page = Math.Min(page, pages);
        return new SongPlaysPage
        {
            Total = total, Page = page, TotalPages = pages,
            Items = await plays.OrderByDescending(p => p.PlayTime).ThenByDescending(p => p.Id).Skip((page - 1) * limit).Take(limit)
                .Select(ToPlay).ToListAsync(HttpContext.RequestAborted),
        };
    }

    /// <summary>
    /// Score per play (oldest first) as at most <paramref name="max"/> points: with more plays, each point is the best
    /// of a run of consecutive plays, so the line keeps its shape and the best play always shows.
    /// </summary>
    public static List<ScorePoint> ScoreLine(IReadOnlyList<(DateTime Time, uint Score)> plays, int max = ScorePoint.Max)
    {
        var per = Math.Max(1, (plays.Count + max - 1) / max);
        return plays.Chunk(per).Select(run =>
        {
            var top = run.MaxBy(p => p.Score);
            return new ScorePoint { PlayTime = top.Time, Score = top.Score, Plays = run.Length };
        }).ToList();
    }

    private static SongChartSummary? Summarize(List<SongChartPlay> plays) => plays.Count == 0 ? null : new SongChartSummary
    {
        Plays = plays.Count, Good = plays.Sum(p => (long)p.Good), Ok = plays.Sum(p => (long)p.Ok), Bad = plays.Sum(p => (long)p.Bad),
        Cleared = plays.Count(p => p.Crown >= CrownType.Clear), FullCombos = plays.Count(p => p.Crown >= CrownType.Gold),
        Donderful = plays.Count(p => p.Crown == CrownType.Dondaful), AverageScore = plays.Average(p => (double)p.Score),
        BestCombo = plays.Max(p => p.Combo), FirstPlayed = plays[0].PlayTime, TopCrown = plays.Max(p => p.Crown),
    };

    private static readonly Expression<Func<SongPlayDatumNijiiro, SongChartPlay>> ToPlay = p => new SongChartPlay
    {
        PlayTime = p.PlayTime, Score = p.Score, Crown = p.Crown, Rank = p.ScoreRank,
        Good = p.GoodCount, Ok = p.OkCount, Bad = p.MissCount, Combo = p.ComboCount, Drumroll = p.DrumrollCount,
    };

    private static int[] Stars(MusicDetail s) => [s.StarEasy, s.StarNormal, s.StarHard, s.StarOni, s.StarUra];
}
