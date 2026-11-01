using System.Globalization;
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
    private static readonly StringComparer TitleOrder = StringComparer.Create(CultureInfo.GetCultureInfo("ja-JP"), false);

    /// <param name="diff">The difficulty (1 Easy … 5 Ura Oni) that <paramref name="status"/> and the stars sort refer to.</param>
    /// <param name="status">all, unplayed, notcleared, nofc; AI Battle: aiunplayed, ailost, aiwon.</param>
    /// <param name="sort">id, title or stars (high first).</param>
    [HttpGet("{baid}")]
    public async Task<ActionResult<SongListPage>> List(uint baid, [FromQuery] string? q = null, [FromQuery] int genre = -1,
        [FromQuery] bool fav = false, [FromQuery] int diff = 4, [FromQuery] string status = "all", [FromQuery] string sort = "id",
        [FromQuery] int page = 1, [FromQuery] int limit = 50)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        var user = await context.UserData.FindAsync([baid], HttpContext.RequestAborted);
        if (user is null) return NotFound();
        if (diff is < 1 or > 5 || page < 1 || limit is < 1 or > MaxLimit) return BadRequest(new { message = "Bad paging or difficulty." });

        var songs = catalog.GetMusicDetailDictionary().Values;
        var best = (await context.SongBestData.Where(b => b.Baid == baid).Select(b => new { b.SongId, b.Difficulty, b.BestCrown })
                .ToListAsync(HttpContext.RequestAborted))
            .ToDictionary(b => (b.SongId, b.Difficulty), b => b.BestCrown);
        var ai = (await context.AiScoreData.Where(a => a.Baid == baid).Include(a => a.AiSectionScoreData).ToListAsync(HttpContext.RequestAborted))
            .ToDictionary(a => (a.SongId, a.Difficulty), a => new AiResult
            {
                Win = a.IsWin, Sections = a.AiSectionScoreData.Count, SectionsWon = a.AiSectionScoreData.Count(s => s.IsWin),
            });
        var favorites = user.FavoriteSongsArray.ToHashSet();
        var d = (Difficulty)diff;

        var query = q?.Trim() ?? "";
        IEnumerable<MusicDetail> list = songs.Where(s =>
            (genre < 0 || (int)s.Genre == genre) &&
            (!fav || favorites.Contains(s.SongId)) &&
            (query.Length == 0 || s.SongId.ToString() == query ||
             s.SongName.Contains(query, StringComparison.OrdinalIgnoreCase) || s.SongNameEN.Contains(query, StringComparison.OrdinalIgnoreCase)));
        if (status != "all")
            list = list.Where(s =>
            {
                if (Stars(s)[diff - 1] == 0) return false;
                var crown = best.TryGetValue((s.SongId, d), out var c) ? c : (CrownType?)null;
                var battle = ai.GetValueOrDefault((s.SongId, d));
                return status switch
                {
                    "unplayed" => crown is null,
                    "notcleared" => crown == CrownType.None,
                    "nofc" => crown is CrownType.None or CrownType.Clear,
                    "aiunplayed" => battle is null,
                    "ailost" => battle is { Win: false },
                    "aiwon" => battle is { Win: true },
                    _ => true,
                };
            });
        list = sort switch
        {
            "title" => list.OrderBy(s => s.SongName, TitleOrder),
            "stars" => list.OrderByDescending(s => Stars(s)[diff - 1]).ThenBy(s => s.SongId),
            _ => list.OrderBy(s => s.SongId),
        };

        var all = list.ToList();
        var pages = Math.Max(1, (all.Count + limit - 1) / limit);
        page = Math.Min(page, pages);
        var shown = all.Skip((page - 1) * limit).Take(limit).ToList();
        // Every player's plays, for the songs on this page only (read from the SongId/Skipped index).
        var ids = shown.Select(s => s.SongId).ToList();
        var totals = await context.SongPlayData.Where(p => ids.Contains(p.SongId) && !p.Skipped)
            .GroupBy(p => p.SongId).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, HttpContext.RequestAborted);
        return new SongListPage
        {
            Total = all.Count, AllSongs = songs.Count, Page = page, TotalPages = pages,
            Items = shown.Select(s => new SongListItem
            {
                SongId = s.SongId, Name = s.SongName, NameEn = s.SongNameEN, Genre = s.Genre, Favorite = favorites.Contains(s.SongId),
                TotalPlays = totals.GetValueOrDefault(s.SongId),
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
        if (user is null || !catalog.GetMusicDetailDictionary().TryGetValue(songId, out var s)) return NotFound();

        // One read of this player's plays of the song (just the numbers, in the order of the Baid/SongId/Difficulty/PlayTime
        // index) gives each chart's totals, best and score line; the plays themselves come a page at a time from Plays().
        var plays = (await context.SongPlayData.Where(p => p.Baid == baid && p.SongId == songId && !p.Skipped)
                .OrderBy(p => p.Difficulty).ThenBy(p => p.PlayTime)
                .Select(p => new { p.Difficulty, Play = new SongChartPlay
                {
                    PlayTime = p.PlayTime, Score = p.Score, Crown = p.Crown, Rank = p.ScoreRank,
                    Good = p.GoodCount, Ok = p.OkCount, Bad = p.MissCount, Combo = p.ComboCount, Drumroll = p.DrumrollCount,
                } }).ToListAsync(HttpContext.RequestAborted))
            .GroupBy(p => p.Difficulty).ToDictionary(g => g.Key, g => g.Select(p => p.Play).ToList());
        var ai = await context.AiScoreData.Where(a => a.Baid == baid && a.SongId == songId).Include(a => a.AiSectionScoreData)
            .ToListAsync(HttpContext.RequestAborted);
        var stars = Stars(s);
        return new SongDetail
        {
            SongId = s.SongId, Name = s.SongName, NameEn = s.SongNameEN, Genre = s.Genre, Stars = stars,
            Favorite = user.FavoriteSongsArray.Contains(songId),
            TotalPlays = await context.SongPlayData.CountAsync(p => p.SongId == songId && !p.Skipped, HttpContext.RequestAborted),
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

    /// <summary>One chart's plays, newest first, a page at a time (the Song page's play history).</summary>
    [HttpGet("{baid}/{songId}/Plays")]
    public async Task<ActionResult<SongPlaysPage>> Plays(uint baid, uint songId, [FromQuery] int difficulty = 4,
        [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        if (difficulty is < 1 or > 5 || page < 1 || limit is < 1 or > MaxLimit) return BadRequest(new { message = "Bad paging or difficulty." });

        var d = (Difficulty)difficulty;
        var plays = context.SongPlayData.Where(p => p.Baid == baid && p.SongId == songId && p.Difficulty == d && !p.Skipped);
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

    private static readonly Expression<Func<SongPlayDatum, SongChartPlay>> ToPlay = p => new SongChartPlay
    {
        PlayTime = p.PlayTime, Score = p.Score, Crown = p.Crown, Rank = p.ScoreRank,
        Good = p.GoodCount, Ok = p.OkCount, Bad = p.MissCount, Combo = p.ComboCount, Drumroll = p.DrumrollCount,
    };

    private static int[] Stars(MusicDetail s) => [s.StarEasy, s.StarNormal, s.StarHard, s.StarOni, s.StarUra];
}
