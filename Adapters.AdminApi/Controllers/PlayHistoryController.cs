namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiScope(ApiScopes.ReadScores, ApiScopes.AdminUsers)]
public class PlayHistoryController(ITaikoDbContext context, IGameDataCatalog catalog) : BaseAdminController<PlayHistoryController>
{
    [HttpGet("{baid}")]
    public async Task<ActionResult<SongHistoryResponse>> GetSongHistory(uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }

        var playLogs = await context.SongPlayData.Where(d => d.Baid == baid).ToListAsync();
        var songHistory = playLogs.Select(play => new SongHistoryData
            {
                SongId = play.SongId,
                Difficulty = play.Difficulty,
                Score = play.Score,
                ScoreRank = play.ScoreRank,
                Crown = play.Crown,
                GoodCount = play.GoodCount,
                OkCount = play.OkCount,
                MissCount = play.MissCount,
                HitCount = play.HitCount,
                DrumrollCount = play.DrumrollCount,
                ComboCount = play.ComboCount,
                PlayTime = play.PlayTime,
                SongNumber = play.SongNumber,
                PlaySetting = PlaySettingConverter.ShortToPlaySetting((short)play.OptionSetting)
            })
            .ToList();

        var favoriteSet = user.FavoriteSongsArray.ToHashSet();
        foreach (var song in songHistory.Where(song => favoriteSet.Contains(song.SongId)))
        {
            song.IsFavorite = true;
        }

        return Ok(new SongHistoryResponse { SongHistoryData = songHistory });
    }

    // Plays further apart than this start a new session.
    private static readonly TimeSpan SessionGap = TimeSpan.FromMinutes(30);

    /// <summary>The calendar: plays per day of <paramref name="year"/>-<paramref name="month"/>.</summary>
    /// <param name="diff">0 = every difficulty, else 1 Easy … 5 Ura Oni.</param>
    /// <param name="best">Only plays that set a new best.</param>
    [HttpGet("{baid}/Month")]
    public async Task<ActionResult<PlayHistoryMonth>> Month(uint baid, [FromQuery] int year, [FromQuery] int month,
        [FromQuery] int diff = 0, [FromQuery] bool best = false)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        if (month is < 1 or > 12 || year is < 2000 or > 9999) return BadRequest(new { message = "Bad month." });

        var start = new DateTime(year, month, 1);
        var inMonth = (await Plays(baid, start, start.AddMonths(1))).Where(p => Matches(p, diff, best)).ToList();
        int total;
        DateTime? latest;
        if (best)
        {
            // shortcut: an all-time count of new bests needs every play in order, so with that filter on the whole
            // history is read (0.4 s at 100,000 plays); a window-function query would avoid it if that ever matters.
            var all = (await Plays(baid, DateTime.MinValue, DateTime.MaxValue)).Where(p => Matches(p, diff, true)).ToList();
            (total, latest) = (all.Count, all.Count == 0 ? null : all.Max(p => p.PlayTime));
        }
        else
        {
            var all = context.SongPlayData.Where(p => p.Baid == baid && !p.Skipped && (diff == 0 || p.Difficulty == (Difficulty)diff));
            total = await all.CountAsync(HttpContext.RequestAborted);
            latest = await all.MaxAsync(p => (DateTime?)p.PlayTime, HttpContext.RequestAborted);
        }
        return new PlayHistoryMonth
        {
            Days = inMonth.GroupBy(p => p.PlayTime.Day).ToDictionary(g => g.Key, g => g.Count()),
            Plays = total,
            Latest = latest?.Date,
        };
    }

    /// <summary>One day's sessions, newest first, each with its matching plays.</summary>
    [HttpGet("{baid}/Day")]
    public async Task<ActionResult<PlayHistoryDay>> Day(uint baid, [FromQuery] DateTime date, [FromQuery] int diff = 0, [FromQuery] bool best = false)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        var day = await Plays(baid, date.Date, date.Date.AddDays(1));
        return new PlayHistoryDay { Sessions = Sessions(day, diff, best) };
    }

    /// <summary>The Dashboard timeline: every session of the last <paramref name="days"/> days (today included), newest first.</summary>
    [HttpGet("{baid}/Recent")]
    public async Task<ActionResult<PlayHistoryDay>> Recent(uint baid, [FromQuery] int days = 7)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        var since = DateTime.Today.AddDays(1 - Math.Clamp(days, 1, 31));
        return new PlayHistoryDay { Sessions = Sessions(await Plays(baid, since, DateTime.MaxValue), 0, false) };
    }

    // Groups plays into sessions (newest first), keeping the plays that match the filters; sessions left empty are dropped.
    private List<PlaySession> Sessions(List<Row> plays, int diff, bool best)
    {
        var songs = catalog.GetMusicDetailDictionary();
        var sessions = new List<List<Row>>();
        foreach (var p in plays.OrderBy(p => p.PlayTime))
        {
            if (sessions.Count == 0 || p.PlayTime - sessions[^1][^1].PlayTime > SessionGap) sessions.Add([]);
            sessions[^1].Add(p);
        }
        return sessions.AsEnumerable().Reverse().Select(s => new PlaySession
        {
            Start = s[0].PlayTime, End = s[^1].PlayTime, Songs = s.Count,
            Credits = NumberCredits(s),
            Plays = s.Where(p => Matches(p, diff, best)).OrderByDescending(p => p.Play.Credit).ThenBy(p => p.SongNumber).Select(p =>
            {
                var song = songs.GetValueOrDefault(p.SongId);
                p.Play.Name = song?.SongName ?? $"#{p.SongId}";
                p.Play.NameEn = song?.SongNameEN ?? "";
                p.Play.Stars = song is null ? 0 : p.Difficulty switch
                {
                    Difficulty.Easy => song.StarEasy, Difficulty.Normal => song.StarNormal, Difficulty.Hard => song.StarHard,
                    Difficulty.Oni => song.StarOni, _ => song.StarUra,
                };
                return p.Play;
            }).ToList(),
        }).Where(s => s.Plays.Count > 0).ToList();
    }

    // Each credit numbers its songs from 0, so a credit starts at song 0, or where the numbering goes back (its
    // first song wasn't saved). Sets each play's Credit and returns how many there were.
    private static int NumberCredits(List<Row> session)
    {
        var credit = 0;
        Row? previous = null;
        foreach (var p in session)
        {
            if (previous is null || p.SongNumber == 0 || p.SongNumber <= previous.SongNumber) credit++;
            p.Play.Credit = credit;
            previous = p;
        }
        return credit;
    }

    private sealed class Row
    {
        public required HistoryPlay Play { get; init; }
        public uint SongNumber { get; init; }
        public DateTime PlayTime => Play.PlayTime;
        public uint SongId => Play.SongId;
        public Difficulty Difficulty => Play.Difficulty;
    }

    private static bool Matches(Row p, int diff, bool best) => (diff == 0 || (int)p.Difficulty == diff) && (!best || p.Play.NewBest);

    // The plays from `from` up to `to`, oldest first, with "new best" worked out per chart. Only those dates are read (a
    // heavy player has 100,000 plays): each shown chart's best before `from` comes from one grouped query.
    private async Task<List<Row>> Plays(uint baid, DateTime from, DateTime to)
    {
        var ct = HttpContext.RequestAborted;
        var rows = await context.SongPlayData.Where(p => p.Baid == baid && !p.Skipped && p.PlayTime >= from && p.PlayTime < to)
            // A credit's songs share one PlayTime (the game saves them together), so the save order (Id) keeps them in
            // song order; without it the order depended on the index SQLite picked and credits were miscounted.
            .OrderBy(p => p.PlayTime).ThenBy(p => p.Id)
            .Select(p => new { p.SongId, p.Difficulty, p.PlayTime, p.Score, p.Crown, p.ScoreRank, p.SongNumber })
            .ToListAsync(ct);
        var songs = rows.Select(p => p.SongId).Distinct().ToList();
        var bestSoFar = songs.Count == 0 ? [] : await context.SongPlayData
            .Where(p => p.Baid == baid && !p.Skipped && p.PlayTime < from && songs.Contains(p.SongId))
            .GroupBy(p => new { p.SongId, p.Difficulty }).Select(g => new { g.Key.SongId, g.Key.Difficulty, Top = g.Max(p => p.Score) })
            .ToDictionaryAsync(g => (g.SongId, g.Difficulty), g => g.Top, ct);
        return rows.Select(p =>
        {
            var key = (p.SongId, p.Difficulty);
            var newBest = bestSoFar.TryGetValue(key, out var prev) && p.Score > prev;
            if (!bestSoFar.ContainsKey(key) || p.Score > prev) bestSoFar[key] = p.Score;
            return new Row
            {
                SongNumber = p.SongNumber,
                Play = new HistoryPlay
                {
                    SongId = p.SongId, Difficulty = p.Difficulty, PlayTime = p.PlayTime, Score = p.Score, Crown = p.Crown, Rank = p.ScoreRank,
                    NewBest = newBest,
                },
            };
        }).ToList();
    }
}
