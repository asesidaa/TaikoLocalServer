namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiScope(ApiScopes.ReadScores, ApiScopes.AdminUsers)]
public partial class PlayHistoryController(ITaikoDbContext context, IGameDataCatalog catalog) : BaseAdminController<PlayHistoryController>
{
    private readonly ITaikoDbContext context = context;

    [HttpGet("{baid}")]
    public Task<ActionResult<SongHistoryResponse>> GetSongHistory(uint baid)
        => GetSongHistory(nameof(GameEra.Nijiiro), baid);

    [HttpGet("/api/{era}/[controller]/{baid}")]
    public async Task<ActionResult<SongHistoryResponse>> GetSongHistory(string era, uint baid)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
            return EraRoute.BadEra(era);

        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }

        return gameEra switch
        {
            GameEra.Nijiiro => Ok(await BuildNijiiroSongHistory(baid)),
            GameEra.Green => Ok(await BuildGreenSongHistory(baid)),
            GameEra.Blue => Ok(await BuildBlueSongHistory(baid)),
            GameEra.Yellow => Ok(await BuildYellowSongHistory(baid)),
            GameEra.Red => Ok(await BuildRedSongHistory(baid)),
            GameEra.White => Ok(await BuildWhiteSongHistory(baid)),
            GameEra.Murasaki => Ok(await BuildMurasakiSongHistory(baid)),
            GameEra.Kimidori => Ok(await BuildKimidoriSongHistory(baid)),
            GameEra.Momoiro => Ok(await BuildMomoiroSongHistory(baid)),
            _ => EraRoute.BadEra(era)
        };
    }

    private async Task<SongHistoryResponse> BuildNijiiroSongHistory(uint baid)
    {
        var saveData = await context.GetOrCreateNijiiroSaveDataAsync(baid, HttpContext.RequestAborted);

        var favoriteSet = saveData.FavoriteSongsArray.ToHashSet();
        var playLogs = await context.SongPlayDataNijiiro
            .Where(d => d.Baid == baid)
            .AsNoTracking()
            .ToListAsync(HttpContext.RequestAborted);
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
                PlaySetting = PlaySettingConverter.ShortToPlaySetting((short)play.OptionSetting),
                IsFavorite = favoriteSet.Contains(play.SongId)
            })
            .ToList();

        return new SongHistoryResponse { SongHistoryData = songHistory };
    }

    private async Task<SongHistoryResponse> BuildGreenSongHistory(uint baid)
    {
        var playLogs = await context.SongPlayDataGreen
            .Where(d => d.Baid == baid)
            .AsNoTracking()
            .ToListAsync(HttpContext.RequestAborted);
        var favoriteSet = await context.GreenFavoriteSongs
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

    private async Task<SongHistoryResponse> BuildBlueSongHistory(uint baid)
    {
        var playLogs = await context.SongPlayDataBlue
            .Where(d => d.Baid == baid)
            .AsNoTracking()
            .ToListAsync(HttpContext.RequestAborted);
        var favoriteSet = await context.BlueFavoriteSongs
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

    // Plays further apart than this start a new session.
    internal static readonly TimeSpan SessionGap = TimeSpan.FromMinutes(30);

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
            // New bests over all time, counted by the database in one pass (the same rule as Plays()).
            (total, latest) = await context.NewBestTotalsAsync(baid, diff == 0 ? null : (Difficulty)diff, HttpContext.RequestAborted);
        }
        else
        {
            var all = context.SongPlayDataNijiiro.Where(p => p.Baid == baid && !p.Skipped && (diff == 0 || p.Difficulty == (Difficulty)diff));
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
        return new PlayHistoryDay { Sessions = Sessions(day, diff, best, await Language(baid)) };
    }

    /// <summary>
    /// The Dashboard timeline: every session of the last <paramref name="days"/> days (today included) in every game,
    /// newest first. A session is on one game's cabinet; older games' sessions come without song names.
    /// </summary>
    [HttpGet("{baid}/Recent")]
    public async Task<ActionResult<PlayHistoryDay>> Recent(uint baid, [FromQuery] int days = 7)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        var since = DateTime.Today.AddDays(1 - Math.Clamp(days, 1, 31));
        var language = await Language(baid);
        var sessions = Sessions(await Plays(baid, since, DateTime.MaxValue), 0, false, language);
        foreach (var (game, rows) in new[]
                 {
                     ("Green", await EraPlays(context.SongPlayDataGreen, baid, since)), ("Blue", await EraPlays(context.SongPlayDataBlue, baid, since)),
                     ("Yellow", await EraPlays(context.SongPlayDataYellow, baid, since)), ("Red", await EraPlays(context.SongPlayDataRed, baid, since)),
                     ("White", await EraPlays(context.SongPlayDataWhite, baid, since)), ("Murasaki", await EraPlays(context.SongPlayDataMurasaki, baid, since)),
                     ("Kimidori", await EraPlays(context.SongPlayDataKimidori, baid, since)), ("Momoiro", await EraPlays(context.SongPlayDataMomoiro, baid, since)),
                 })
            sessions.AddRange(Sessions(rows, 0, false, language, game));
        return new PlayHistoryDay { Sessions = sessions.OrderByDescending(s => s.Start).ToList() };
    }

    // Groups plays into sessions (newest first), keeping the plays that match the filters; sessions left empty are dropped.
    private Task<uint> Language(uint baid) => SongLanguage.ForAsync(context, User, baid, HttpContext.RequestAborted);

    private List<PlaySession> Sessions(List<Row> plays, int diff, bool best, uint language, string game = "Nijiiro")
    {
        var songs = game == "Nijiiro" ? catalog.Nijiiro().GetMusicDetailDictionary() : [];
        var sessions = new List<List<Row>>();
        foreach (var p in plays.OrderBy(p => p.PlayTime))
        {
            if (sessions.Count == 0 || p.PlayTime - sessions[^1][^1].PlayTime > SessionGap) sessions.Add([]);
            sessions[^1].Add(p);
        }
        return sessions.AsEnumerable().Reverse().Select(s => new PlaySession
        {
            Game = game,
            Start = s[0].PlayTime, End = s[^1].PlayTime, Songs = s.Count,
            Credits = NumberCredits(s),
            Plays = s.Where(p => Matches(p, diff, best)).OrderByDescending(p => p.Play.Credit).ThenBy(p => p.SongNumber).Select(p =>
            {
                var song = songs.GetValueOrDefault(p.SongId);
                p.Play.Name = song?.Title(language) ?? (game == "Nijiiro" ? $"#{p.SongId}" : "");
                p.Play.NameEn = song?.Subtitle(language) ?? "";
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

    // An older game's plays from `from` on, as Rows. The game saves a credit's songs in one request, so they share a
    // PlayTime: each song's number within it starts a new credit at 0, as Nijiiro's SongNumber does. "New best" is
    // against every earlier play of the chart, as Plays works it out.
    private async Task<List<Row>> EraPlays<T>(IQueryable<T> table, uint baid, DateTime from) where T : class, IAc15SongPlayDatum
    {
        var ct = HttpContext.RequestAborted;
        var rows = await table.Where(p => p.Baid == baid && p.PlayTime >= from)
            .OrderBy(p => p.PlayTime).ThenBy(p => EF.Property<long>(p, "Id"))
            .Select(p => new { p.SongId, p.Difficulty, p.PlayTime, p.Score, p.Crown })
            .ToListAsync(ct);
        if (rows.Count == 0) return [];
        var songs = rows.Select(p => p.SongId).Distinct().ToList();
        var bestSoFar = await table.Where(p => p.Baid == baid && p.PlayTime < from && songs.Contains(p.SongId))
            .GroupBy(p => new { p.SongId, p.Difficulty }).Select(g => new { g.Key.SongId, g.Key.Difficulty, Top = g.Max(p => p.Score) })
            .ToDictionaryAsync(g => (g.SongId, g.Difficulty), g => g.Top, ct);
        uint number = 0;
        return rows.Select((p, i) =>
        {
            number = i > 0 && rows[i - 1].PlayTime == p.PlayTime ? number + 1 : 0;
            var key = (p.SongId, p.Difficulty);
            var newBest = bestSoFar.TryGetValue(key, out var prev) && p.Score > prev;
            if (!bestSoFar.ContainsKey(key) || p.Score > prev) bestSoFar[key] = p.Score;
            return new Row
            {
                SongNumber = number,
                Play = new HistoryPlay { SongId = p.SongId, Difficulty = p.Difficulty, PlayTime = p.PlayTime, Score = p.Score, Crown = p.Crown, NewBest = newBest },
            };
        }).ToList();
    }

    private static bool Matches(Row p, int diff, bool best) => (diff == 0 || (int)p.Difficulty == diff) && (!best || p.Play.NewBest);

    // The plays from `from` up to `to`, oldest first, with "new best" worked out per chart. Only those dates are read (a
    // heavy player has 100,000 plays): each shown chart's best before `from` comes from one grouped query.
    private async Task<List<Row>> Plays(uint baid, DateTime from, DateTime to)
    {
        var ct = HttpContext.RequestAborted;
        var rows = await context.SongPlayDataNijiiro.Where(p => p.Baid == baid && !p.Skipped && p.PlayTime >= from && p.PlayTime < to)
            // A credit's songs share one PlayTime (the game saves them together), so the save order (Id) keeps them in
            // song order; without it the order depended on the index SQLite picked and credits were miscounted.
            .OrderBy(p => p.PlayTime).ThenBy(p => p.Id)
            .Select(p => new { p.SongId, p.Difficulty, p.PlayTime, p.Score, p.Crown, p.ScoreRank, p.SongNumber })
            .ToListAsync(ct);
        var songs = rows.Select(p => p.SongId).Distinct().ToList();
        var bestSoFar = songs.Count == 0 ? [] : await context.SongPlayDataNijiiro
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
