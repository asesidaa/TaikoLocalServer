
namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>
/// Play Data › High Scores: a player's best on every chart of one difficulty, filtered, sorted and paged here.
/// Comparing with another player needs access to theirs too: admins, or anyone in Local mode.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiScope(ApiScopes.ReadScores, ApiScopes.AdminUsers)]
public class HighScoresController(ITaikoDbContext context, IGameDataCatalog catalog) : BaseAdminController<HighScoresController>
{

    /// <param name="diff">1 Easy … 5 Ura Oni.</param>
    /// <param name="crown">-1 any, else the crown (0 not cleared … 3 donderful); AI Battle: 0 lost, 1 won.</param>
    /// <param name="sort">stars, score, date (recently set) or title.</param>
    /// <param name="vs">Another player's Baid to compare with; 0 = none.</param>
    /// <param name="behind">Comparing: only the charts where they're ahead.</param>
    /// <param name="ai">AI Battle results instead (score: its sections' total); no comparing or date.</param>
    [HttpGet("{baid}")]
    public async Task<ActionResult<HighScoresPage>> Get(uint baid, [FromQuery] int diff = 4, [FromQuery] string? q = null,
        [FromQuery] int genre = -1, [FromQuery] int crown = -1, [FromQuery] string sort = "stars", [FromQuery] uint vs = 0,
        [FromQuery] bool behind = false, [FromQuery] int page = 1, [FromQuery] int limit = 50, [FromQuery] bool ai = false)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        if (vs != 0 && this.AuthorizeOwnerOrAdmin(vs) is { } forbidVs)
            return forbidVs;
        if (diff is < 1 or > 5 || page < 1 || limit is < 1 or > 200) return BadRequest(new { message = "Bad paging or difficulty." });

        if (ai) vs = 0;
        var d = (Difficulty)diff;
        var songs = catalog.Nijiiro().GetMusicDetailDictionary();
        var mine = ai ? [] : await context.SongBestDataNijiiro.Where(b => b.Baid == baid).ToListAsync(HttpContext.RequestAborted);
        var battles = !ai ? [] : await context.AiScoreDataNijiiro.Where(a => a.Baid == baid).Include(a => a.AiSectionScoreData).ToListAsync(HttpContext.RequestAborted);
        var theirs = vs == 0 ? [] : (await context.SongBestDataNijiiro.Where(b => b.Baid == vs && b.Difficulty == d).ToListAsync(HttpContext.RequestAborted))
            .ToDictionary(b => b.SongId);
        // The play that set each best: when, and its accuracy (a Good counts 1, an OK ½, over all its notes), as the
        // song page and Stats count it. The game's own rate goes past 100%.
        var bestPlays = (await context.SongPlayDataNijiiro.Where(p => p.Baid == baid && p.Difficulty == d && !p.Skipped)
                .Select(p => new { p.SongId, p.Score, p.PlayTime, p.GoodCount, p.OkCount, p.MissCount }).ToListAsync(HttpContext.RequestAborted))
            .GroupBy(p => p.SongId).ToDictionary(g => g.Key, g => g.MaxBy(p => p.Score)!);
        double AccuracyOf(SongBestDatumNijiiro b) =>
            bestPlays.TryGetValue(b.SongId, out var p) && p.GoodCount + p.OkCount + p.MissCount is var notes and > 0
                ? (p.GoodCount + p.OkCount / 2.0) / notes * 100
                : Math.Min(b.BestRate / 100.0, 100);   // no play recorded (an imported best): the game's rate, capped

        var query = q?.Trim() ?? "";
        var language = await SongLanguage.ForAsync(context, User, baid, HttpContext.RequestAborted);
        var rows = mine.Where(b => b.Difficulty == d && songs.ContainsKey(b.SongId)).Select(b =>
        {
            var s = songs[b.SongId];
            var other = theirs.GetValueOrDefault(b.SongId);
            return new HighScoreRow
            {
                SongId = b.SongId, Name = s.Title(language), NameEn = s.Subtitle(language), Genre = s.Genre, Stars = Stars(s, d),
                Score = b.BestScore, Crown = b.BestCrown, Rank = b.BestScoreRank, Accuracy = AccuracyOf(b),
                SetOn = bestPlays.TryGetValue(b.SongId, out var at) ? at.PlayTime : null,
                VsScore = other?.BestScore, VsCrown = other?.BestCrown,
            };
        }).Concat(battles.Where(a => a.Difficulty == d && songs.ContainsKey(a.SongId)).Select(a =>
        {
            var s = songs[a.SongId];
            return new HighScoreRow
            {
                SongId = a.SongId, Name = s.Title(language), NameEn = s.Subtitle(language), Genre = s.Genre, Stars = Stars(s, d),
                Score = (uint)a.AiSectionScoreData.Sum(x => x.Score),
                Ai = new AiResult { Win = a.IsWin, Sections = a.AiSectionScoreData.Count, SectionsWon = a.AiSectionScoreData.Count(x => x.IsWin) },
            };
        })).Where(r => (genre < 0 || (int)r.Genre == genre) &&
                      songs[r.SongId].Matches(query))
            .ToList();

        var both = rows.Where(r => r.VsScore is not null).ToList();
        int Kind(HighScoreRow r) => ai ? (r.Ai?.Win == true ? 1 : 0) : (int)r.Crown;
        var shown = rows.Where(r => (crown < 0 || Kind(r) == crown) && (!behind || r.VsScore > r.Score));
        shown = sort switch
        {
            "score" => shown.OrderByDescending(r => r.Score),
            "date" when !ai => shown.OrderByDescending(r => r.SetOn),
            "title" => shown.OrderBy(r => r.Name, SongLanguage.TitleOrder(language)),
            _ => shown.OrderByDescending(r => r.Stars).ThenByDescending(r => r.Score),
        };
        var list = shown.ToList();
        var pages = Math.Max(1, (list.Count + limit - 1) / limit);
        page = Math.Min(page, pages);
        return new HighScoresPage
        {
            Items = list.Skip((page - 1) * limit).Take(limit).ToList(),
            Total = list.Count, Page = page, TotalPages = pages,
            PlayedPerDifficulty = Enumerable.Range(1, 5).Select(i => ai
                ? battles.Count(a => (int)a.Difficulty == i && songs.ContainsKey(a.SongId))
                : mine.Count(b => (int)b.Difficulty == i && songs.ContainsKey(b.SongId))).ToArray(),
            Charts = songs.Values.Count(s => Stars(s, d) > 0),
            Crowns = Enumerable.Range(0, 4).Select(c => rows.Count(r => Kind(r) == c)).ToArray(),
            VsName = vs == 0 ? null : (await context.UserData.FindAsync([vs], HttpContext.RequestAborted))?.MyDonName,
            VsAhead = both.Count(r => r.Score > r.VsScore),
            VsBehind = both.Count(r => r.Score < r.VsScore),
            VsBoth = both.Count,
        };
    }

    private static int Stars(MusicDetail s, Difficulty d) => d switch
    {
        Difficulty.Easy => s.StarEasy, Difficulty.Normal => s.StarNormal, Difficulty.Hard => s.StarHard, Difficulty.Oni => s.StarOni, _ => s.StarUra,
    };
}
