using System.Globalization;
using Microsoft.JSInterop;

namespace TaikoWebUI.Pages;

// Computed client-side from api/PlayData (best per song × difficulty) and api/PlayHistory (every play).
// shortcut: pulls the whole play history; move the aggregation to a server Stats endpoint when histories get large.
public partial class Stats
{
    [Parameter] public int Baid { get; set; }

    private UserSetting? setting;
    private List<DanBestData> dans = [];
    private List<SongBestData> best = [];
    private List<SongHistoryData> history = [];
    private Dictionary<uint, MusicDetail> music = [];
    private bool loaded;

    private static readonly Difficulty[] Courses = [Difficulty.Easy, Difficulty.Normal, Difficulty.Hard, Difficulty.Oni, Difficulty.UraOni];

    // Sequential one-hue ramp in the picked accent for magnitude; step 0 = no plays. Separate light / dark steps (dark
    // brightens with more plays). Only the accent steps every theme defines (100, 300-700).
    internal static readonly string[] Seq =
    [
        "bg-zinc-100 dark:bg-zinc-800", "bg-primary-100 dark:bg-primary-700/45", "bg-primary-300 dark:bg-primary-600/80",
        "bg-primary-500 dark:bg-primary-500", "bg-primary-700 dark:bg-primary-300",
    ];

    // Validated categorical trio for Good / OK / Bad (not the game's judgement colours, which fail CVD separation).
    private static readonly string[] Acc = ["bg-[#eb6834] dark:bg-[#d95926]", "bg-[#1baf7a] dark:bg-[#199e70]", "bg-[#2a78d6] dark:bg-[#3987e5]"];

    protected override async Task OnInitializedAsync()
    {
        Page.Set(Localizer["Play Data"], Localizer["Stats"]);
        var settingTask = Client.GetFromJsonAsync<UserSetting>($"api/UserSettings/{Baid}");
        var dansTask = Client.GetFromJsonAsync<DanBestDataResponse>($"api/DanBestData/{Baid}");
        var bestTask = Client.GetFromJsonAsync<SongBestResponse>($"api/PlayData/{Baid}");
        var historyTask = Client.GetFromJsonAsync<SongHistoryResponse>($"api/PlayHistory/{Baid}");
        music = await GameDataService.GetMusicDetailDictionary();
        setting = await settingTask;
        dans = (await dansTask)?.DanBestDataList ?? [];
        best = (await bestTask)?.SongBestData ?? [];
        history = (await historyTask)?.SongHistoryData ?? [];
        loaded = true;
    }

    protected override Task OnAfterRenderAsync(bool firstRender) => Js.InvokeVoidAsync("ui.hydrateAssets").AsTask();

    private string SongName(uint id) => GameDataService.GetMusicNameBySongId(music, id, CultureInfo.CurrentCulture.ToString()) is { Length: > 0 } n ? n : $"#{id}";

    // ---- Tiles ----
    private int SongsPlayed => best.Select(b => b.SongId).Distinct().Count();
    private string ClearRate => best.Count == 0 ? "–" : $"{best.Count(b => b.BestCrown >= CrownType.Clear) * 100 / best.Count}%";
    private int FullCombos => best.Count(b => b.BestCrown >= CrownType.Gold);
    private int Donderfuls => best.Count(b => b.BestCrown == CrownType.Dondaful);
    private uint BestCombo => history.Count == 0 ? 0 : history.Max(h => h.ComboCount);
    private int PlaysThisWeek => history.Count(h => h.PlayTime >= DateTime.Now.Date.AddDays(-6));

    // ---- Scoreboard (the in-game achievement panel): same counting as the game ----
    // None = the highest difficulty with plays; Ura Oni also counts Oni.
    private Difficulty PanelDifficulty
    {
        get
        {
            var d = setting?.AchievementDisplayDifficulty ?? Difficulty.None;
            if (d != Difficulty.None) return d;
            return Courses.LastOrDefault(c => best.Any(b => b.Difficulty == c), Difficulty.Oni);
        }
    }

    private int[] PanelCounts()
    {
        var d = PanelDifficulty;
        var rows = best.Where(b => b.Difficulty == d || (d == Difficulty.UraOni && b.Difficulty == Difficulty.Oni)).ToList();
        int Rank(ScoreRank r) => rows.Count(b => b.BestScoreRank == r);
        int Crown(CrownType c) => rows.Count(b => b.BestCrown == c);
        // Slot order: 極, 雅 gold/pink/purple, 粋 white/bronze/silver, crowns clear/full combo/donderful.
        return [Rank(ScoreRank.Dondaful), Rank(ScoreRank.Gold), Rank(ScoreRank.Sakura), Rank(ScoreRank.Purple),
                Rank(ScoreRank.White), Rank(ScoreRank.Bronze), Rank(ScoreRank.Silver),
                Crown(CrownType.Clear), Crown(CrownType.Gold), Crown(CrownType.Dondaful)];
    }

    // Number centres on the 328×192 panel (from the game's player_board layout).
    private static readonly (int X, int Y)[] Slots = [(278, 35), (88, 74), (180, 74), (278, 74), (88, 112), (180, 112), (278, 112), (88, 155), (180, 155), (278, 155)];
    private static readonly int[][] DigitOffsets = [[0], [-7, 7], [-14, 0, 14], [-21, -7, 7, 21]];

    private static string PanelName(Difficulty d) => d switch
    {
        Difficulty.Easy => "easy", Difficulty.Normal => "normal", Difficulty.Hard => "hard", Difficulty.UraOni => "oniura", _ => "oni",
    };

    private static string Pct(double v, double of) => FormattableString.Invariant($"{v / of * 100:0.###}%");

    // ---- Crowns by difficulty ----
    private (int Played, int Clear, int Full, int Donderful) Crowns(Difficulty d)
    {
        var rows = best.Where(b => b.Difficulty == d).ToList();
        return (rows.Count, rows.Count(b => b.BestCrown >= CrownType.Clear), rows.Count(b => b.BestCrown >= CrownType.Gold), rows.Count(b => b.BestCrown == CrownType.Dondaful));
    }

    // ---- Clears by star level, for the picked difficulty: cleared vs available ----
    private Difficulty starsDifficulty = Difficulty.Oni;

    private (int Cleared, int Available)[] StarLevels()
    {
        var result = new (int, int)[10];
        foreach (var id in music.Keys)
        {
            var star = GameDataService.GetMusicStarLevel(music, id, starsDifficulty);
            if (star is >= 1 and <= 10) result[star - 1].Item2++;
        }
        foreach (var b in best.Where(b => b.Difficulty == starsDifficulty && b.BestCrown >= CrownType.Clear))
        {
            var star = GameDataService.GetMusicStarLevel(music, b.SongId, starsDifficulty);
            if (star is >= 1 and <= 10) result[star - 1].Item1++;
        }
        return result;
    }

    // ---- Activity: the last year, one column per week (Monday first), up to today ----
    private const int ActivityWeeks = 53;

    private (DateTime Day, int Plays)[] Activity()
    {
        var today = DateTime.Now.Date;
        var start = today.AddDays(-(((int)today.DayOfWeek + 6) % 7) - 7 * (ActivityWeeks - 1));
        var counts = history.Where(h => h.PlayTime >= start).GroupBy(h => h.PlayTime.Date).ToDictionary(g => g.Key, g => g.Count());
        return Enumerable.Range(0, (today - start).Days + 1).Select(i => start.AddDays(i)).Select(d => (d, counts.GetValueOrDefault(d))).ToArray();
    }

    private static int Bucket(int v) => v switch { 0 => 0, <= 2 => 1, <= 4 => 2, <= 6 => 3, _ => 4 };

    // ---- Genres, accuracy, most played (from every play) ----
    private List<(SongGenre Genre, int Plays)> Genres() => history
        .GroupBy(h => GameDataService.GetMusicGenreBySongId(music, h.SongId)).Select(g => (g.Key, g.Count()))
        .OrderByDescending(g => g.Item2).ToList();

    private (string Label, double Pct)[] Accuracy()
    {
        double good = history.Sum(h => (double)h.GoodCount), ok = history.Sum(h => (double)h.OkCount), bad = history.Sum(h => (double)h.MissCount);
        var total = good + ok + bad;
        if (total == 0) return [];
        return [(Localizer["Good"], good / total * 100), (Localizer["OK"], ok / total * 100), (Localizer["Bad"], bad / total * 100)];
    }

    private List<(uint SongId, int Plays)> MostPlayed() => history.GroupBy(h => h.SongId)
        .Select(g => (g.Key, g.Count())).OrderByDescending(g => g.Item2).Take(5).ToList();
}
