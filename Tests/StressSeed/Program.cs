// A large, realistic database for load testing, built through the real EF model and migrations (README.md).
//   dotnet run --project Tests/StressSeed -- [--players 5000] [--plays 500000] [--whales 10] [--top 100000] [--seed 1] [--out <file>] [--data <wwwroot>]
// Or one demo player added to an existing database (stop the server first; delete them later in User Management):
//   dotnet run --project Tests/StressSeed -- --into <taiko.db3> --name keifun [--plays 3000] [--password <pw>] [--data <wwwroot>]
//   They're an admin, registered, with a new access code; both are printed. No password given: a random one.
// Same seed, same database. Song ids, star levels, note counts and max scores come from the extracted
// datatables (data/datatable/musicinfo.json etc.), so charts, scores and crowns are believable.
using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Domain;
using TaikoLocalServer.Domain.Entities;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog;
using TaikoLocalServer.Infrastructure.Persistence;

var opt = ParseArgs(args);
var root = Path.GetFullPath(opt.GetValueOrDefault("data") ?? Path.Combine(RepoRoot(), "Host/bin/Debug/net10.0/wwwroot"));
var outPath = Path.GetFullPath(opt.GetValueOrDefault("out") ?? Path.Combine(root, "taiko-stress.db3"));
var playerCount = int.Parse(opt.GetValueOrDefault("players") ?? "5000");
var playTarget = int.Parse(opt.GetValueOrDefault("plays") ?? "500000");
var rng = new Random(int.Parse(opt.GetValueOrDefault("seed") ?? "1"));
var into = opt.GetValueOrDefault("into") is { } intoArg ? Path.GetFullPath(intoArg) : null;
if (into is not null && !File.Exists(into)) { Console.Error.WriteLine($"No database at {into}."); return 1; }
var clock = Stopwatch.StartNew();

// ---- Game data ----
var charts = LoadCharts(root);
var cos = LoadCostumes(root);
var dans = LoadDans(root);
Console.WriteLine($"{charts.Count} charts, {dans.Count} dan courses from {root}");
if (charts.Count == 0) { Console.Error.WriteLine("No songs: point --data at a wwwroot with extracted datatables."); return 1; }

// Song popularity: a few songs get most plays (Zipf over a shuffled order).
var songIds = charts.Select(c => c.SongId).Distinct().OrderBy(_ => rng.Next()).ToArray();
var popularity = songIds.Select((_, i) => 1.0 / Math.Pow(i + 1, 0.9)).ToArray();
var popularityTotal = popularity.Sum();
var chartsBySong = charts.GroupBy(c => c.SongId).ToDictionary(g => g.Key, g => g.ToList());

// ---- Database ----
// Built as <this exe>/wwwroot/taiko.db3 and moved to --out at the end: an early migration (SeparateTokens) opens its
// own context on that default path, so migrating any other file fails.
var work = Path.Combine(PathHelper.GetRootPath(), "taiko.db3");
TaikoDbContext db;
if (into is null)
{
    Directory.CreateDirectory(Path.GetDirectoryName(work)!);
    if (File.Exists(work)) File.Delete(work);
    await using (var setup = new TaikoDbContext(work)) await setup.Database.MigrateAsync();
    Console.WriteLine($"Migrated ({clock.Elapsed.TotalSeconds:0.0}s)");
    Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();   // the migrations' pooled connections would block the PRAGMAs
    db = new TaikoDbContext(work);
    await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=OFF; PRAGMA synchronous=OFF;");
}
else
{
    // A real server's database: written normally (journaled), and only the new player's rows.
    db = new TaikoDbContext(into);
    if ((await db.Database.GetPendingMigrationsAsync()).Any()) { Console.Error.WriteLine("Start the server once first: this database needs migrating."); return 1; }
}
db.ChangeTracker.AutoDetectChangesEnabled = false;

// One password for every registered player: hashing thousands with BCrypt would take minutes.
var Password = into is null ? "stresstest" : opt.GetValueOrDefault("password") ?? Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(6)).ToLowerInvariant();
var salt = BCrypt.Net.BCrypt.GenerateSalt(10);
var hash = BCrypt.Net.BCrypt.HashPassword(Password, salt);

var now = DateTime.Now;
var start = now.AddYears(-2);
var accessCodes = into is null ? new HashSet<string>() : (await db.Cards.Select(c => c.AccessCode).ToListAsync()).ToHashSet();
string Digits(int n) => string.Concat(Enumerable.Range(0, n).Select(_ => (char)('0' + rng.Next(10))));
string NewCode(HashSet<string> taken, int n) { string c; do c = Digits(n); while (!taken.Add(c)); return c; }

var players = new List<(uint Baid, double Skill, int Plays, DateTime From, DateTime To)>();
// --into: one strong player, active the whole two years, after the database's last player.
if (into is not null)
    players.Add(((await db.UserData.MaxAsync(u => (uint?)u.Baid) ?? 0) + 1, 0.75, int.Parse(opt.GetValueOrDefault("plays") ?? "3000"), start, now));
for (uint baid = 1; into is null && baid <= playerCount; baid++)
{
    // Plays per player: a long tail (most play a little, a few play thousands), scaled to the target total.
    var plays = Math.Max(1, (int)Math.Round(Math.Exp(Normal(3.8, 1.3))));
    var skill = Math.Clamp(Normal(0.45, 0.22), 0.02, 0.99);
    var from = start.AddDays(rng.NextDouble() * 680);
    var to = from.AddDays(Math.Min((now - from).TotalDays, 7 + plays * (0.3 + rng.NextDouble())));
    players.Add((baid, skill, plays, from, to));
}
var scale = into is null ? (double)playTarget / players.Sum(p => p.Plays) : 1;
players = players.Select(p => p with { Plays = Math.Max(1, (int)Math.Round(p.Plays * scale)) }).ToList();

// Heavy players, on top of the target: players 2…whales+1, from --top plays (100,000) down to a tenth of it (evenly spaced on a log
// scale), strong, active the whole two years, and registered, with access codes 00…010001, 00…010002, ….
var whales = into is not null ? 0 : Math.Min(int.Parse(opt.GetValueOrDefault("whales") ?? "10"), playerCount - 1);
bool IsWhale(uint baid) => baid >= 2 && baid <= whales + 1;
string WhaleCode(uint baid) => (10_000 + baid - 1).ToString("D20");
var top = int.Parse(opt.GetValueOrDefault("top") ?? "100000");
for (var i = 0; i < whales; i++)
{
    var plays = (int)Math.Round(top / 10.0 * Math.Pow(10, whales == 1 ? 1 : (double)(whales - 1 - i) / (whales - 1)));
    players[i + 1] = players[i + 1] with { Plays = plays, Skill = 0.55 + 0.4 * rng.NextDouble(), From = start, To = now };
}

var names = new[] { "どん", "かっ", "Don", "Katsu", "たいこ", "Taiko", "ばち", "Bachi", "りずむ", "Rhythm", "みこし", "Mikoshi", "ドンちゃん", "Kaks", "Raccoon", "Haru", "Mika", "Daichi", "りまう", "Sora" };
foreach (var (baid, skill, playCount, _, to) in players)
{
    var admin = into is not null || baid == 1;
    db.UserData.Add(new UserDatum
    {
        Baid = baid, MyDonName = into is not null ? opt.GetValueOrDefault("name") ?? "keifun" : admin ? "admin" : IsWhale(baid) ? $"Heavy{baid - 1}" : $"{names[rng.Next(names.Length)]}{rng.Next(1, 1000)}",
        MyDonNameLanguage = into is not null ? 1 : (uint)rng.Next(2), IsAdmin = admin,
    });
    db.UserSaveDataNijiiro.Add(new UserSaveDataNijiiro
    {
        Baid = baid, DisplayDan = true, DisplayAchievement = true, AchievementDisplayDifficulty = Difficulty.None,
        ColorBody = (uint)rng.Next(63), ColorFace = (uint)rng.Next(63), ColorLimb = (uint)rng.Next(63),
        CurrentKigurumi = rng.Next(6) == 0 ? Pick(cos["kigurumi"]) : 0, CurrentHead = Pick(cos["head"]), CurrentBody = Pick(cos["body"]),
        CurrentFace = Pick(cos["face"]), CurrentPuchi = Pick(cos["puchi"]),
        FavoriteSongsArray = Enumerable.Range(0, rng.Next(12)).Select(_ => songIds[rng.Next(songIds.Length)]).Distinct().ToList(),
        LastPlayDatetime = to, DisplaySouUchi = rng.Next(10) == 0,
    });
    db.UserWebProfiles.Add(new UserWebProfile
    {
        Baid = baid, CardColor = (uint)rng.Next(63),
        CardPattern = (CardPattern)rng.Next(Enum.GetValues<CardPattern>().Length), CardAnimation = (CardAnimation)rng.Next(Enum.GetValues<CardAnimation>().Length),
    });
    var cards = admin || IsWhale(baid) ? 1 : rng.Next(10) < 8 ? 1 : rng.Next(2, 4);
    for (var i = 0; i < cards; i++)
        db.Cards.Add(new Card { AccessCode = into is not null ? NewCode(accessCodes, 20) : admin ? "00000000000000000001" : IsWhale(baid) ? WhaleCode(baid) : NewCode(accessCodes, 20), Baid = baid, LastScanAt = to });
    var registered = admin || IsWhale(baid) || rng.Next(10) < 3;
    db.Credentials.Add(new Credential { Baid = baid, Password = registered ? hash : "", Salt = registered ? salt : "" });
}
await Save("players");

// ---- Plays: credits of 3–4 songs on play days, skill-matched charts, accuracy from skill vs stars ----
var bests = new Dictionary<(uint, uint, Difficulty), SongBestDatumNijiiro>();
var aiDone = new HashSet<(uint, uint, Difficulty)>();
long playRows = 0;
foreach (var (baid, skill, playCount, from, to) in players)
{
    var left = playCount;
    var skillStar = 1 + skill * 9.5;   // the star level this player clears comfortably
    while (left > 0)
    {
        var time = from.AddSeconds(rng.NextDouble() * Math.Max(1, (to - from).TotalSeconds));
        time = time.Date.AddHours(11 + rng.Next(12)).AddMinutes(rng.Next(60));
        var songsInCredit = Math.Min(left, rng.Next(3) == 0 ? 4 : 3);
        for (var n = 0; n < songsInCredit; n++)
        {
            var chart = PickChart(skillStar);
            var play = Play(chart, skillStar);
            db.SongPlayDataNijiiro.Add(new SongPlayDatumNijiiro
            {
                Baid = baid, SongNumber = (uint)n, SongId = chart.SongId, Difficulty = chart.Difficulty, Crown = play.Crown,
                Score = play.Score, ScoreRate = play.Rate, ScoreRank = play.Rank, GoodCount = play.Good, OkCount = play.Ok,
                MissCount = play.Bad, ComboCount = play.Combo, HitCount = play.Good + play.Ok + play.Rolls, DrumrollCount = play.Rolls,
                Skipped = rng.Next(200) == 0, PlayTime = time,
            });
            var key = (baid, chart.SongId, chart.Difficulty);
            if (!bests.TryGetValue(key, out var best))
                bests[key] = best = new SongBestDatumNijiiro { Baid = baid, SongId = chart.SongId, Difficulty = chart.Difficulty };
            best.UpdateBestData(play.Crown, (uint)play.Rank, play.Score, play.Rate);

            // Some charts get an AI Battle too (3 or 5 sections, from the song's length).
            if (chart.Difficulty >= Difficulty.Hard && rng.Next(40) == 0) AddAiBattle(baid, chart, play);
        }
        left -= songsInCredit;
        playRows += songsInCredit;
        if (playRows % 50_000 < songsInCredit) await Save($"{playRows:N0} plays");
    }
}
db.SongBestDataNijiiro.AddRange(bests.Values);
await Save($"{playRows:N0} plays, {bests.Count:N0} bests");

// ---- Dani Dojo: players attempt the dan courses around their skill ----
var danRows = 0;
foreach (var (baid, skill, playCount, _, _) in players.Where(p => p.Plays > 30))
{
    var level = (int)(skill * dans.Count);
    foreach (var dan in dans.Where(d => Math.Abs(dans.IndexOf(d) - level) <= 2))
    {
        var reach = Math.Clamp(skill * dans.Count - dans.IndexOf(dan) + 1.5 + Normal(0, 0.7), 0, 3);
        var state = reach < 1.6 ? DanClearState.NotClear : reach < 2.2 ? DanClearState.RedNormalClear : reach < 2.6 ? DanClearState.GoldNormalClear : reach < 2.9 ? DanClearState.GoldFullComboClear : DanClearState.GoldPerfectClear;
        var datum = new DanScoreDatumNijiiro { Baid = baid, DanId = dan.Id, DanType = DanType.Normal, ClearState = state, ArrivalSongCount = (uint)Math.Clamp((int)Math.Ceiling(reach), 1, dan.Songs.Count) };
        for (var i = 0; i < datum.ArrivalSongCount; i++)
        {
            var chart = charts.FirstOrDefault(c => c.SongId == dan.Songs[i].SongId && c.Difficulty == dan.Songs[i].Difficulty) ?? charts[0];
            var p = Play(chart, 1 + skill * 9.5);
            datum.DanStageScoreData.Add(new DanStageScoreDatumNijiiro
            {
                Baid = baid, DanId = dan.Id, DanType = DanType.Normal, SongNumber = (uint)i, PlayScore = p.Score, HighScore = p.Score,
                GoodCount = p.Good, OkCount = p.Ok, BadCount = p.Bad, ComboCount = p.Combo, DrumrollCount = p.Rolls, TotalHitCount = p.Good + p.Ok + p.Rolls,
            });
            datum.ComboCountTotal += p.Combo;
        }
        datum.SoulGaugeTotal = (uint)(datum.ArrivalSongCount * 90);
        db.DanScoreDataNijiiro.Add(datum);
        danRows++;
    }
}
await Save($"{danRows:N0} dan results");

if (into is not null)
{
    var (newBaid, _, newPlays, _, _) = players[0];
    var code = await db.Cards.Where(c => c.Baid == newBaid).Select(c => c.AccessCode).FirstAsync();
    await db.DisposeAsync();
    Console.WriteLine($"Done in {clock.Elapsed.TotalSeconds:0.0}s: added player {newBaid} to {into}, an admin, with {newPlays:N0} plays.");
    Console.WriteLine($"Sign in with access code {code}, password \"{Password}\". Remove them in User Management when you're done.");
    return 0;
}

// ---- The admin activity log and sign-ins, so the Access pages have history to page through ----
var actions = new[] { "user.login", "user.password", "user.theme", "user.costume", "server.avatars", "card.bind" };
for (var i = 0; i < Math.Min(playerCount * 10, 100_000); i++)
{
    var who = (uint)rng.Next(1, playerCount + 1);
    var at = start.AddSeconds(rng.NextDouble() * (now - start).TotalSeconds);
    db.Activity.Add(new ActivityEntry { Time = at, ActorBaid = who, TargetBaid = who, Action = actions[rng.Next(actions.Length)] });
    if (i % 2 == 0) db.SignInAttempts.Add(new SignInAttempt { Time = at, Baid = who, AccessCode = "", Result = (SignInOutcome)(rng.Next(10) == 0 ? 1 : 0), Ip = $"10.0.{rng.Next(256)}.{rng.Next(256)}", UserAgent = "StressSeed" });
}
await Save("activity log");

await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=DELETE; VACUUM; ANALYZE;");
await db.Database.CloseConnectionAsync();
await db.DisposeAsync();
Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
// A -wal/-shm left beside an older file at --out would be replayed over the new one (it'd look empty).
foreach (var leftover in new[] { "-wal", "-shm" }) File.Delete(outPath + leftover);
File.Move(work, outPath, overwrite: true);
Console.WriteLine($"Done in {clock.Elapsed.TotalSeconds:0.0}s: {outPath} ({new FileInfo(outPath).Length / 1048576.0:0.0} MB)");
Console.WriteLine($"Admin: access code 00000000000000000001, password \"{Password}\" (about 30% of players share that password).");
foreach (var p in players.Where(p => IsWhale(p.Baid)))
    Console.WriteLine($"  Heavy{p.Baid - 1} (player {p.Baid}): {p.Plays:N0} plays, access code {WhaleCode(p.Baid)}");
return 0;

// ---------------------------------------------------------------------------------------------------------------

async Task Save(string what)
{
    db.ChangeTracker.DetectChanges();
    await db.SaveChangesAsync();
    db.ChangeTracker.Clear();
    Console.WriteLine($"  {what} ({clock.Elapsed.TotalSeconds:0.0}s)");
}

// A chart near the player's level: a popular song, then the difficulty whose stars sit closest to their skill.
Chart PickChart(double skillStar)
{
    var r = rng.NextDouble() * popularityTotal;
    var i = 0;
    while (i < popularity.Length - 1 && (r -= popularity[i]) > 0) i++;
    var options = chartsBySong[songIds[i]];
    return options.MinBy(c => Math.Abs(c.Stars - (skillStar + Normal(0, 1.3))))!;
}

// One play: the further the chart is above the player's level, the more OKs and Bads.
(uint Good, uint Ok, uint Bad, uint Combo, uint Rolls, uint Score, uint Rate, CrownType Crown, ScoreRank Rank) Play(Chart c, double skillStar)
{
    var gap = c.Stars - skillStar + Normal(0, 0.8);
    var badRate = Math.Clamp(0.01 + 0.035 * Math.Max(0, gap + 1.5) + 0.01 * gap * Math.Abs(gap), 0, 0.6);
    var okRate = Math.Clamp(0.06 + 0.04 * (gap + 2), 0.005, 0.5);
    var bad = (uint)Math.Round(c.Notes * badRate * rng.NextDouble() * 2);
    if (rng.NextDouble() < Math.Clamp(-gap / 6, 0, 0.35)) bad = 0;   // comfortable charts get full combos
    var ok = (uint)Math.Min(c.Notes - bad, Math.Round(c.Notes * okRate * (0.5 + rng.NextDouble())));
    if (bad == 0 && rng.NextDouble() < Math.Clamp(-gap / 10, 0, 0.12)) ok = 0;
    var good = (uint)c.Notes - bad - ok;
    var hit = (good + ok / 2.0) / c.Notes;
    var rolls = (uint)rng.Next(0, 40);
    var score = (uint)Math.Min(c.MaxScore, Math.Round(c.MaxScore * hit + rolls * 100));
    var cleared = hit > (c.Difficulty switch { Difficulty.Easy => 0.6, Difficulty.Normal => 0.65, Difficulty.Hard => 0.7, _ => 0.75 }) - 0.05 * Normal(0, 1);
    var crown = !cleared ? CrownType.None : bad > 0 ? CrownType.Clear : ok > 0 ? CrownType.Gold : CrownType.Dondaful;
    var rank = score >= 1_000_000 ? ScoreRank.Dondaful : score >= 950_000 ? ScoreRank.Purple : score >= 900_000 ? ScoreRank.Sakura
        : score >= 800_000 ? ScoreRank.Gold : score >= 700_000 ? ScoreRank.Silver : score >= 600_000 ? ScoreRank.Bronze : score >= 500_000 ? ScoreRank.White : ScoreRank.None;
    var combo = bad == 0 ? (uint)c.Notes : (uint)Math.Round(c.Notes / (bad + 1.0) * (1 + rng.NextDouble()));
    return (good, ok, bad, Math.Min(combo, (uint)c.Notes), rolls, score, (uint)Math.Round(hit * 10000), crown, rank);
}

void AddAiBattle(uint baid, Chart chart, (uint Good, uint Ok, uint Bad, uint Combo, uint Rolls, uint Score, uint Rate, CrownType Crown, ScoreRank Rank) play)
{
    if (!aiDone.Add((baid, chart.SongId, chart.Difficulty))) return;   // one battle per chart
    var sections = chart.Notes > 600 ? 5 : 3;
    var ai = new AiScoreDatumNijiiro { Baid = baid, SongId = chart.SongId, Difficulty = chart.Difficulty };
    var won = 0;
    for (var i = 0; i < sections; i++)
    {
        var win = rng.NextDouble() < 0.35 + play.Rate / 20000.0;
        won += win ? 1 : 0;
        ai.AiSectionScoreData.Add(new AiSectionScoreDatumNijiiro
        {
            Baid = baid, SongId = chart.SongId, Difficulty = chart.Difficulty, SectionIndex = i, IsWin = win, Crown = play.Crown,
            Score = play.Score / (uint)sections, GoodCount = play.Good / (uint)sections, OkCount = play.Ok / (uint)sections,
            MissCount = play.Bad / (uint)sections, DrumrollCount = play.Rolls / (uint)sections,
        });
    }
    ai.IsWin = won * 2 > sections;
    db.AiScoreDataNijiiro.Add(ai);
}

uint Pick(List<uint> ids) => ids.Count == 0 ? 0 : ids[rng.Next(ids.Count)];

double Normal(double mean, double sd) =>
    mean + sd * Math.Sqrt(-2 * Math.Log(1 - rng.NextDouble())) * Math.Cos(2 * Math.PI * rng.NextDouble());

static Dictionary<string, string> ParseArgs(string[] a)
{
    var d = new Dictionary<string, string>();
    for (var i = 0; i + 1 < a.Length; i += 2) d[a[i].TrimStart('-')] = a[i + 1];
    return d;
}

static string RepoRoot()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "TaikoLocalServer.slnx")) && !Directory.Exists(Path.Combine(dir.FullName, ".git"))) dir = dir.Parent;
    return dir?.FullName ?? Directory.GetCurrentDirectory();
}

static JsonElement Items(string path)
{
    var doc = JsonDocument.Parse(File.ReadAllText(path)).RootElement;
    return doc.ValueKind == JsonValueKind.Object && doc.TryGetProperty("items", out var items) ? items : doc;
}

// Playable charts: songs in music_order, up to the server's default song id limit, with their real stars and notes.
static List<Chart> LoadCharts(string root)
{
    var dt = Path.Combine(root, "data", "datatable");
    var ordered = Items(Path.Combine(dt, "music_order.json")).EnumerateArray().Select(e => e.GetProperty("uniqueId").GetUInt32()).ToHashSet();
    var list = new List<Chart>();
    foreach (var e in Items(Path.Combine(dt, "musicinfo.json")).EnumerateArray())
    {
        var id = e.GetProperty("uniqueId").GetUInt32();
        if (id == 0 || id > DomainConstants.MusicIdMax || !ordered.Contains(id)) continue;
        foreach (var (diff, key) in new[] { (Difficulty.Easy, "Easy"), (Difficulty.Normal, "Normal"), (Difficulty.Hard, "Hard"), (Difficulty.Oni, "Mania"), (Difficulty.UraOni, "Ura") })
        {
            var stars = e.GetProperty($"star{key}").GetInt32();
            var notes = e.GetProperty($"{char.ToLower(key[0])}{key[1..]}OnpuNum").GetInt32();
            var max = e.GetProperty($"shinutiScore{key}").GetInt32();
            if (stars > 0 && notes > 0) list.Add(new Chart(id, diff, stars, notes, Math.Max(max, 900_000)));
        }
    }
    return list;
}

static Dictionary<string, List<uint>> LoadCostumes(string root)
{
    var byType = new[] { "kigurumi", "head", "body", "face", "puchi" }.ToDictionary(t => t, _ => new List<uint>());
    foreach (var e in Items(Path.Combine(root, "data", "datatable", "don_cos_reward.json")).EnumerateArray())
        if (byType.TryGetValue(e.GetProperty("cosType").GetString() ?? "", out var ids)) ids.Add(e.GetProperty("uniqueId").GetUInt32());
    return byType;
}

static List<Dan> LoadDans(string root)
{
    var path = Path.Combine(root, "data", "dan_data.json");
    if (!File.Exists(path)) return [];
    return Items(path).EnumerateArray().Select(e => new Dan(e.GetProperty("danId").GetUInt32(),
        e.GetProperty("aryOdaiSong").EnumerateArray().Select(s => (s.GetProperty("songNo").GetUInt32(), (Difficulty)s.GetProperty("level").GetInt32())).ToList())).ToList();
}

record Chart(uint SongId, Difficulty Difficulty, int Stars, int Notes, int MaxScore);

record Dan(uint Id, List<(uint SongId, Difficulty Difficulty)> Songs);

