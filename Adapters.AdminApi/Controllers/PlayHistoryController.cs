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

        var plays = (await Plays(baid)).Where(p => Matches(p, diff, best)).ToList();
        var inMonth = plays.Where(p => p.PlayTime.Year == year && p.PlayTime.Month == month).ToList();
        return new PlayHistoryMonth
        {
            Days = inMonth.GroupBy(p => p.PlayTime.Day).ToDictionary(g => g.Key, g => g.Count()),
            Plays = plays.Count,
            Latest = plays.Count == 0 ? null : plays.Max(p => p.PlayTime).Date,
        };
    }

    /// <summary>One day's sessions, newest first, each with its matching plays.</summary>
    [HttpGet("{baid}/Day")]
    public async Task<ActionResult<PlayHistoryDay>> Day(uint baid, [FromQuery] DateTime date, [FromQuery] int diff = 0, [FromQuery] bool best = false)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        var day = (await Plays(baid)).Where(p => p.PlayTime.Date == date.Date).ToList();
        return new PlayHistoryDay { Sessions = Sessions(day, diff, best) };
    }

    /// <summary>The Dashboard timeline: every session of the last <paramref name="days"/> days (today included), newest first.</summary>
    [HttpGet("{baid}/Recent")]
    public async Task<ActionResult<PlayHistoryDay>> Recent(uint baid, [FromQuery] int days = 7)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        var since = DateTime.Today.AddDays(1 - Math.Clamp(days, 1, 31));
        return new PlayHistoryDay { Sessions = Sessions((await Plays(baid)).Where(p => p.PlayTime >= since).ToList(), 0, false) };
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
            // Each credit numbers its songs from 0.
            Credits = Math.Max(1, s.Count(p => p.SongNumber == 0)),
            Plays = s.Where(p => Matches(p, diff, best)).Reverse().Select(p =>
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

    private sealed class Row
    {
        public required HistoryPlay Play { get; init; }
        public uint SongNumber { get; init; }
        public DateTime PlayTime => Play.PlayTime;
        public uint SongId => Play.SongId;
        public Difficulty Difficulty => Play.Difficulty;
    }

    private static bool Matches(Row p, int diff, bool best) => (diff == 0 || (int)p.Difficulty == diff) && (!best || p.Play.NewBest);

    // Every play, oldest first, with "new best" worked out per chart. One user's plays are a few thousand rows at most.
    private async Task<List<Row>> Plays(uint baid)
    {
        var rows = await context.SongPlayData.Where(p => p.Baid == baid && !p.Skipped).OrderBy(p => p.PlayTime)
            .Select(p => new { p.SongId, p.Difficulty, p.PlayTime, p.Score, p.Crown, p.ScoreRank, p.SongNumber })
            .ToListAsync(HttpContext.RequestAborted);
        var bestSoFar = new Dictionary<(uint, Difficulty), uint>();
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
