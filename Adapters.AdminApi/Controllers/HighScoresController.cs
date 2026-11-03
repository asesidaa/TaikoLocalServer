
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
    /// <param name="crown">-1 any, else the crown (0 not cleared … 3 donderful).</param>
    /// <param name="sort">stars, score, date (recently set) or title.</param>
    /// <param name="vs">Another player's Baid to compare with; 0 = none.</param>
    /// <param name="behind">Comparing: only the charts where they're ahead.</param>
    [HttpGet("{baid}")]
    public async Task<ActionResult<HighScoresPage>> Get(uint baid, [FromQuery] int diff = 4, [FromQuery] string? q = null,
        [FromQuery] int genre = -1, [FromQuery] int crown = -1, [FromQuery] string sort = "stars", [FromQuery] uint vs = 0,
        [FromQuery] bool behind = false, [FromQuery] int page = 1, [FromQuery] int limit = 50)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        if (vs != 0 && this.AuthorizeOwnerOrAdmin(vs) is { } forbidVs)
            return forbidVs;
        if (diff is < 1 or > 5 || page < 1 || limit is < 1 or > 200) return BadRequest(new { message = "Bad paging or difficulty." });

        var d = (Difficulty)diff;
        var songs = catalog.Nijiiro().GetMusicDetailDictionary();
        var mine = await context.SongBestDataNijiiro.Where(b => b.Baid == baid).ToListAsync(HttpContext.RequestAborted);
        var theirs = vs == 0 ? [] : (await context.SongBestDataNijiiro.Where(b => b.Baid == vs && b.Difficulty == d).ToListAsync(HttpContext.RequestAborted))
            .ToDictionary(b => b.SongId);
        // When each best was set: the play with that score.
        var setOn = (await context.SongPlayDataNijiiro.Where(p => p.Baid == baid && p.Difficulty == d && !p.Skipped)
                .Select(p => new { p.SongId, p.Score, p.PlayTime }).ToListAsync(HttpContext.RequestAborted))
            .GroupBy(p => p.SongId).ToDictionary(g => g.Key, g => g.MaxBy(p => p.Score)!.PlayTime);

        var query = q?.Trim() ?? "";
        var language = await SongLanguage.ForAsync(context, User, baid, HttpContext.RequestAborted);
        var rows = mine.Where(b => b.Difficulty == d && songs.ContainsKey(b.SongId)).Select(b =>
        {
            var s = songs[b.SongId];
            var other = theirs.GetValueOrDefault(b.SongId);
            return new HighScoreRow
            {
                SongId = b.SongId, Name = s.Title(language), NameEn = s.Subtitle(language), Genre = s.Genre, Stars = Stars(s, d),
                Score = b.BestScore, Crown = b.BestCrown, Rank = b.BestScoreRank, Accuracy = b.BestRate / 100.0,
                SetOn = setOn.TryGetValue(b.SongId, out var at) ? at : null,
                VsScore = other?.BestScore, VsCrown = other?.BestCrown,
            };
        }).Where(r => (genre < 0 || (int)r.Genre == genre) &&
                      (query.Length == 0 || r.SongId.ToString() == query ||
                       r.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || r.NameEn.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var both = rows.Where(r => r.VsScore is not null).ToList();
        var shown = rows.Where(r => (crown < 0 || (int)r.Crown == crown) && (!behind || r.VsScore > r.Score));
        shown = sort switch
        {
            "score" => shown.OrderByDescending(r => r.Score),
            "date" => shown.OrderByDescending(r => r.SetOn),
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
            PlayedPerDifficulty = Enumerable.Range(1, 5).Select(i => mine.Count(b => (int)b.Difficulty == i && songs.ContainsKey(b.SongId))).ToArray(),
            Charts = songs.Values.Count(s => Stars(s, d) > 0),
            Crowns = Enumerable.Range(0, 4).Select(c => rows.Count(r => (int)r.Crown == c)).ToArray(),
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
